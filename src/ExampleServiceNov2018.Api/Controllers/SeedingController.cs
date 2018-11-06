using System;
using System.Threading.Tasks;
using System.Timers;
using ExampleServiceNov2018.Application;
using ExampleServiceNov2018.Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ExampleServiceNov2018.Api.Controllers
{
    [Route("test")]
    public class SeedingController : Controller
    {
        private readonly IMediator _mediator;

        public SeedingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("seed/{qty}")]
        public async Task<IActionResult> Seed([FromRoute]int qty)
        {
            //sequential writes
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var ticks = DateTimeOffset.UtcNow.Ticks;
            
            for (int i = 0; i < qty; i++)
            {
                               
                var listId = $"Todo{ticks+i}";
                await _mediator.Send(Cmd.Of(new NameTodoList{Name = listId, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new AddTodoItem{ItemText= "a",ItemNumber = 1, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new AddTodoItem{ItemText= "a2",ItemNumber = 2, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new CheckTodoItem{ItemNumber = 1, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new CheckTodoItem{ItemNumber = 2, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new UncheckTodoItem{ItemNumber= 1, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new AddTodoItem{ItemText= "a3",ItemNumber = 3, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new CheckTodoItem {ItemNumber = 3, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new CheckTodoItem {ItemNumber = 1, AggregateId = listId}));
                await _mediator.Send(Cmd.Of(new NameTodoList(){Name = "Closed", AggregateId = listId}));
            }

            watch.Stop();
            return Ok(watch.Elapsed.ToString());

        }
    }
}