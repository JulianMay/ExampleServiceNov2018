using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SqlStreamStore;

namespace ExampleServiceNov2018.ReadService
{
    /// <summary>
    /// Runs the background listening of async sql-projectionss
    /// </summary>
    public class TodoReadServiceHost : IHostedService
    {
        private readonly string _sqlConnectionString;
        private readonly IStreamStore _streamStore;
        private SqlProjectionSubscription[] _subscriptions;

        public TodoReadServiceHost(string sqlConnectionString, IStreamStore streamStore)
        {
            _sqlConnectionString = sqlConnectionString;
            _streamStore = streamStore;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = SqlProjectionFactory.Prepare(_sqlConnectionString, _streamStore);
            var todoListProjection = new TodoLists();
            ISqlProjection[] projections = {todoListProjection};
            _subscriptions = factory.WakeReadProjections(projections).ToArray();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_subscriptions.Select(s => s.UnSubscribe()));
        }
    }
}