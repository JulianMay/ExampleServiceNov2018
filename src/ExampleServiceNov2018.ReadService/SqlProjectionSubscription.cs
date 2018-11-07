using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SqlStreamStore;
using SqlStreamStore.Streams;
using SqlStreamStore.Subscriptions;

namespace ExampleServiceNov2018.ReadService
{
    /// <summary>
    ///     Wraps the subscription to the eventstreams, and handles commit-interval (rarely while catching-up, instantly on
    ///     catched-up),
    ///     as well as maintaining a read-position with the
    /// </summary>
    internal class SqlProjectionSubscription
    {
        private readonly ISqlProjection _projection;
        private readonly SqlSubscriptionPersistence _persistence;
        private readonly IStreamStore _store;
        private StringBuilder _dmlCollector = new StringBuilder();
        private volatile static object commitLock = new object();
        
        private long? _lastReadPosition;
        private long? _lastCommitPosition;
        private bool _runningLive;
        private IAllStreamSubscription _subscription;
        private int commitsInThisLifeCycle = 0;
        
        //Performance-testing
        private DateTimeOffset _startedCatchingUpAt;

        public SqlProjectionSubscription(IStreamStore store, ISqlProjection projection, SqlSubscriptionPersistence persistence)
        {
            _store = store;
            _projection = projection;
            _persistence = persistence;
            _lastReadPosition = persistence.InitialReadPosition;
            _runningLive = false;
        }

        public void Subscribe()
        {
            _subscription = _store.SubscribeToAll(_lastReadPosition, OnEvent, OnSubscriptionDropped, OnCatchUpStatus, true,
                _projection.SchemaIdentifier.Name);
            _startedCatchingUpAt = DateTimeOffset.Now;
        }

        public Task UnSubscribe()
        {
            //capture to avoid race-conditions
            var oldSubscription = _subscription;
            if (oldSubscription == null)
                return Task.CompletedTask;

            //No unsubscruption behavior in StreamStore? ... probably just need to find it...
            return Task.CompletedTask;
        }

        private async Task OnEvent(IAllStreamSubscription subscription, StreamMessage msg,
            CancellationToken cancelToken)
        {
            _lastReadPosition = msg.Position;

            try
            {
                var @event = await Deserialization.Deserialize(msg);

                var change = _projection.Apply(@event);
                if (change != string.Empty)
                    _dmlCollector.AppendLine(change);
                
                await CommitIfRelevant(ProjectionPosition.From(subscription, msg, _lastCommitPosition));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"WHOOPS: {e}");
            }
        }

        private static CommitBatchPolicy BatchingPolicy = new CommitBatchPolicy(500);

        private async Task CommitIfRelevant(ProjectionPosition p)
        {
            if (!BatchingPolicy.ShouldCommit(p))
                return;

            await CommitState();
            if (p.LooksLikeLatestAvailableInput())
            {
                //performance testing:
                var now = DateTimeOffset.UtcNow;
                var timeSpentCatchingUp = now - _startedCatchingUpAt;                
                Console.WriteLine($"Time taken to restore state of projection: {timeSpentCatchingUp.TotalSeconds} seconds");                
            }
                
        }
        
        private async Task CommitState()
        {
            if (_lastReadPosition == null)
                return;

            _lastCommitPosition = await _persistence.CommitToPersistence(_dmlCollector, _lastReadPosition);  
            _dmlCollector = new StringBuilder();
            ++commitsInThisLifeCycle;
        }
        
        

        private void OnSubscriptionDropped(IAllStreamSubscription subscription, SubscriptionDroppedReason reason,
            Exception exception)
        {
            throw new NotImplementedException("No error-handling in this POC");
        }

        
        private void OnCatchUpStatus(bool isCatchedUp)
        {
            if (_runningLive == isCatchedUp)
                return;

            _runningLive = isCatchedUp;

//            if (isCatchedUp)
//            {
//                //performance testing:
//                var timeSpentCatchingUp = DateTimeOffset.UtcNow - _startedCatchingUpAt;
//                Console.WriteLine($"Time taken to restore state of projection: {timeSpentCatchingUp.TotalSeconds} seconds");
//            }
//            else
//            {
//                _startedCatchingUpAt = DateTimeOffset.UtcNow;
//            }
        }
    }
}