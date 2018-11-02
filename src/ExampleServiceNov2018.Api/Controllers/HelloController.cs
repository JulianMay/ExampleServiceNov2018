using System.Threading.Tasks;
using ExampleServiceNov2018.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ExampleServiceNov2018.Api.Controllers
{
    [Route("hello")]
    public class HelloController : Controller
    {
        private readonly IMediator _mediator;

        public HelloController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("")]
        public string Index()
        {
            return "Hello world";
        }

        [HttpGet("{name}")]
        public async Task<string> Greet(string name)
            => await _mediator.Send(new GetGreeting(name));

    }
}