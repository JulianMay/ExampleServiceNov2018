using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SqlStreamStore.Subscriptions;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace ExampleServiceNov2018.ReadService
{   
    /// <summary>
    /// Wraps the subscription to the eventstreams, and handles commit-interval (rarely while catching-up, instantly on catched-up),
    /// as well as maintaining a read-position with the 
    /// </summary>
    public class SqlProjectionSubscription
    {
        private readonly IStreamStore _store;
        private readonly ISqlProjection _projection;
        private readonly string _sqlConnString;
        
        private long _readPosition;
        private bool _runningLive;
        private StringBuilder _dmlCollector = new StringBuilder();
        private IAllStreamSubscription _subscription;

        public SqlProjectionSubscription(IStreamStore store, ISqlProjection projection, long initialReadPosition, string sqlConnString)
        {
            _store = store;
            _projection = projection;
            _readPosition = initialReadPosition;
            _sqlConnString = sqlConnString;
            _runningLive = false;
        }

        public void Subscribe()
        {
            _subscription = _store.SubscribeToAll(_readPosition, OnEvent, OnSubscriptionDropped, OnCatchUpStatus, true, _projection.SchemaIdentifier);
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

        private async Task OnEvent(IAllStreamSubscription subscription, StreamMessage msg, CancellationToken cancelToken)
        {
            _readPosition = msg.Position;
            
            var @event = await Deserialization.Deserialize(msg);
            
            var change = _projection.Apply(@event);
            if(change != string.Empty)
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
            using (var readDb = SqlExecution.OpenWriteConnection(_sqlConnString))
            {
                //Consider a write-lock around _dmlCollector, is it necessary?
                //Consider pre-pending a "BEGIN TRANSACTION" and have a all-or-nothing write
                _dmlCollector.AppendLine(
                    $"UPDATE Inf_ReadSubscriptions SET ReadPosition = {_readPosition} WHERE SchemaIdentifier = '{_projection.SchemaIdentifier}';");
                var dml = new SqlCommand(_dmlCollector.ToString(), readDb);
                var effect = await dml.ExecuteNonQueryAsync();
                if(effect == 0)
                    throw new InvalidOperationException("Something went wrong while updating the state of a readservice. SQL:\r\n" + _dmlCollector);
                
                //todo: Error handling?
                
                _dmlCollector = new StringBuilder();
                
            }
        }
        
        private void OnSubscriptionDropped(IAllStreamSubscription subscription, SubscriptionDroppedReason reason, Exception exception)
        {
            throw new NotImplementedException("No error-handling in this POC");
        }


        private void OnCatchUpStatus(bool isCatchedUp)
        {
            if(_runningLive == isCatchedUp)
                return;
            
            _runningLive = isCatchedUp;
            
            if(isCatchedUp)
                CommitState().GetAwaiter().GetResult();

        }

        
    }
}
