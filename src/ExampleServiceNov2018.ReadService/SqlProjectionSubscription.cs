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
        private long? _uncomittedReadCount;
        private bool _runningLive;
        private IAllStreamSubscription _subscription;
        
        //Performance-testing
        private DateTimeOffset _subscribedAt;

        public SqlProjectionSubscription(IStreamStore store, ISqlProjection projection, SqlSubscriptionPersistence persistence)
        {
            _store = store;
            _projection = projection;
            _persistence = persistence;
            _lastReadPosition = persistence.InitialReadPosition;
            _uncomittedReadCount = 0;
            _runningLive = false;
        }

        public void Subscribe()
        {
            _subscription = _store.SubscribeToAll(_lastReadPosition, OnEvent, OnSubscriptionDropped, OnCatchUpStatus, true,
                _projection.SchemaIdentifier.Name);
            _subscribedAt = DateTimeOffset.Now;
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

            var @event = await Deserialization.Deserialize(msg);

            var change = _projection.Apply(@event);
            if (change != string.Empty)
                _dmlCollector.AppendLine(change);

            await CommitIfRelevant();
        }

        private Task CommitIfRelevant()
        {
            if (_runningLive || _uncomittedReadCount > 1000)
                return CommitState();

            ++_uncomittedReadCount;
            return Task.CompletedTask;
        }
                //RACE CONDITION ... :/ Need a state-machine for commits i think...
        private async Task CommitState()
        {
//            lock (commitLock)
//            {
                if (_lastReadPosition == null)
                    return;

                await _persistence.CommitToPersistence(_dmlCollector, _lastReadPosition);
                _uncomittedReadCount = 0;
            //}
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

            if (isCatchedUp)
            {
                CommitState().GetAwaiter().GetResult();
                
                //performance testing:
                var timeSpentCatchingUp = DateTimeOffset.Now - _subscribedAt;
                Console.WriteLine($"Time taken to restore state of projection: {timeSpentCatchingUp.TotalSeconds} seconds");
            }
        }
    }
}