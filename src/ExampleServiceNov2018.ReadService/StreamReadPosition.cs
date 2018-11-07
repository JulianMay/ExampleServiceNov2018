using System;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace ExampleServiceNov2018.ReadService
{
    public struct ProjectionPosition
    {
        public readonly long? LastRead;
        public readonly long? LastCommit;
        public readonly long? LastKnownHeadOfStream;

        public bool LooksLikeLatestAvailableInput() => LastRead.HasValue && LastRead.IsEqualTo(LastKnownHeadOfStream);
        public long UncommitedLength => LastRead.DistanceDownTo(LastCommit);
        
        public static ProjectionPosition From(IAllStreamSubscription allStream, StreamMessage msg, long? latestCommit)
            => new ProjectionPosition(msg.Position, latestCommit, allStream.LatestKnownHead);                

        public ProjectionPosition(long? lastRead, long? lastCommit, long? lastKnownHeadOfStream)
        {
            if(lastCommit.IsHigherThan(lastKnownHeadOfStream))
                throw new ArgumentException("Commit position cannot be higher than streamlength, something is wrong", nameof(lastCommit));
            if(lastCommit.IsHigherThan(lastRead))
                throw new ArgumentException("Commit position cannot be higher than lastRead, something is wrong", nameof(lastRead));
            
            LastRead = lastRead;
            LastCommit = lastCommit;
            LastKnownHeadOfStream = lastKnownHeadOfStream;
        }
        
        
    }

    public static class NullableExtensions
    {
        public static bool IsHigherThan(this long? self, long? other) =>
            (self.HasValue && other.HasValue) && self > other;

        public static bool IsEqualTo(this long? self, long? other)
        {
            if (!self.HasValue && !other.HasValue)
                return true;

            if (!other.HasValue)
                return false;
            
            return self.Equals(other);
        }

        public static long DistanceDownTo(this long? self, long? other)
        {
            var a = self ?? -1;
            var b = other ?? -1;
            return a - b;
        }
    }

    public struct CommitBatchPolicy
    {
        public readonly long EventsPerCommitWhenCatchingUp;

        public CommitBatchPolicy(long eventsPerCommitWhenCatchingUp)
        {
            EventsPerCommitWhenCatchingUp = eventsPerCommitWhenCatchingUp;
        }
        //public readonly int EventsPerCommitWhenInputBuffered; 
        
        //^-- callbacks for "onCatchupStatus" are dispatched as soon as the latest event in
        //the inputstream has been buffered, this is an "almost there"-state


        public bool ShouldCommit(ProjectionPosition p)
        { 
            return p.LooksLikeLatestAvailableInput() || p.UncommitedLength >= EventsPerCommitWhenCatchingUp;
        } 


        //This is a quick test for "ShouldCommit"
        public static void AssertShouldCommit()
        {
            var smallBatch = new CommitBatchPolicy(10);
            var bigBatch = new CommitBatchPolicy(1000);
            
            var reloadPositionCatchedUp = new ProjectionPosition(444,440,444);
            var rebuildFirstPositionCatchedUp = new ProjectionPosition(444,null,444);

            var reloadCatchingUp900Uncomitted = new ProjectionPosition(999, 99, 8747235);
            
            if(smallBatch.ShouldCommit(reloadPositionCatchedUp) == false)
                throw new InvalidOperationException("failed expectation: small batch to commit on "+nameof(reloadPositionCatchedUp));
            
            if(bigBatch.ShouldCommit(reloadPositionCatchedUp) == false)
                throw new InvalidOperationException("failed expectation: big batch to commit on "+nameof(reloadPositionCatchedUp));
            
            if(smallBatch.ShouldCommit(rebuildFirstPositionCatchedUp) == false)
                throw new InvalidOperationException("failed expectation: small batch to commit on "+nameof(rebuildFirstPositionCatchedUp));
            
            if(smallBatch.ShouldCommit(rebuildFirstPositionCatchedUp) == false)
                throw new InvalidOperationException("failed expectation: big batch to commit on "+nameof(rebuildFirstPositionCatchedUp));
            
            if(smallBatch.ShouldCommit(reloadCatchingUp900Uncomitted) == false)
                throw new InvalidOperationException("failed expectation: small batch to commit on "+nameof(reloadCatchingUp900Uncomitted));
            
            if(bigBatch.ShouldCommit(reloadCatchingUp900Uncomitted))
                throw new InvalidOperationException("failed expectation: big batch not to commit on "+nameof(reloadCatchingUp900Uncomitted));
        }
    }
}