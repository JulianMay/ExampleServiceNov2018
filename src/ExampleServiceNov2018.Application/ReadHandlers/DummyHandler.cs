using System;
using System.Threading;
using System.Threading.Tasks;
using ExampleServiceNov2018.Application.Queries;
using MediatR;

namespace ExampleServiceNov2018.Application
{
    public class DummyHandler : IRequestHandler<GetGreeting, string>
    {
        public Task<string> Handle(GetGreeting request, CancellationToken cancellationToken)
            => Task.FromResult($"Hello {request.Name} :)");
    }
}
