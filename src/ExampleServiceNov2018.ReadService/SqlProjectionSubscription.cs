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

        private long? _readPosition;
        private bool _runningLive;
        private IAllStreamSubscription _subscription;
        
        //Performance-testing
        private DateTimeOffset _subscribedAt;

        public SqlProjectionSubscription(IStreamStore store, ISqlProjection projection, SqlSubscriptionPersistence persistence)
        {
            _store = store;
            _projection = projection;
            _persistence = persistence;
            _readPosition = persistence.InitialReadPosition;
            _runningLive = false;
        }

        public void Subscribe()
        {
            _subscription = _store.SubscribeToAll(_readPosition, OnEvent, OnSubscriptionDropped, OnCatchUpStatus, true,
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
            _readPosition = msg.Position;

            var @event = await Deserialization.Deserialize(msg);

            var change = _projection.Apply(@event);
            if (change != string.Empty)
                _dmlCollector.AppendLine(change);

            await CommitIfRelevant();
        }

        private Task CommitIfRelevant()
        {
            if (_runningLive || _readPosition % 1000 == 0)
                return CommitState();
            return Task.CompletedTask;
        }

        private async Task CommitState()
        {
            if (_readPosition == null)
                return;
            
            await _persistence.CommitToPersistence(_dmlCollector, _readPosition);
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