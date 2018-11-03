using System;
using System.Text;
using SqlStreamStore.Subscriptions;
using SqlStreamStore;

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
        private readonly long _readPosition;

        private StringBuilder _dmlCollector = new StringBuilder();

        public SqlProjectionSubscription(IStreamStore store, ISqlProjection projection, long initialReadPosition)
        {
            _store = store;
            _projection = projection;
            _readPosition = initialReadPosition;
        }
        
    }
}
