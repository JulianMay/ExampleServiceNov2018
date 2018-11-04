using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ExampleServiceNov2018.Application.Queries;

namespace ExampleServiceNov2018.Api.Controllers
{
    [Route("todo/read")]
    public class TodoReadController : Controller
    {
        private readonly IMediator _mediator;

        public TodoReadController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("All")]
        public async Task<TodoLists> GetAll() => await _mediator.Send(new ListAllItems());
        
        [HttpGet("ById/{aggregateId}")]
        public async Task<TodoLists.List> GetByIId([FromRoute]string aggregateId) 
            => await _mediator.Send(new GetTodoItemById{AggregateId = aggregateId});
    }
}