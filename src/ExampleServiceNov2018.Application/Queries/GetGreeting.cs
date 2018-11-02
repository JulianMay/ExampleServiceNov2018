using MediatR;

namespace ExampleServiceNov2018.Application.Queries
{
    public class GetGreeting : IRequest<string>
    {
        public readonly string Name;

        public GetGreeting(string name)
        {
            Name = name;
        }
    }
}