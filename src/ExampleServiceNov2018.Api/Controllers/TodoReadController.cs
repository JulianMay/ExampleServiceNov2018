using System.Threading.Tasks;
using ExampleServiceNov2018.ReadService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<TodoListCollectionDTO> GetAll() => await _mediator.Send(new ListAllItems());
        
        [HttpGet("ById/{aggregateId}")]
        public async Task<TodoListDTO> GetByIId([FromRoute]string aggregateId) 
            => await _mediator.Send(new GetTodoListById{AggregateId = aggregateId});
    }
}