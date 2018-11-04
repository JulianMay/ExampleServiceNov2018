using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ExampleServiceNov2018.Application.Queries
{
    public class TodoLists
    {
        public List[] Collection { get; set; }
        
        public class List
        {
            public string AggregateId { get; set; }
            public string Name { get; set; }
            public Item[] Items { get; set; }
        }
        
        public class Item
        {
            public string Text { get; set; }
            public bool Checked { get; set; }
        }
    }
    
    public class ListAllItems : IRequest<TodoLists> { }
    public class GetTodoItemById : IRequest<TodoLists.List> { public string AggregateId; }



    public class QueryHandler :
        IRequestHandler<ListAllItems, TodoLists>,
        IRequestHandler<GetTodoItemById, TodoLists.List>
    {
        private readonly ITodoListReadService _read;

        public QueryHandler(ITodoListReadService read)
        {
            _read = read;
        }

        public Task<TodoLists> Handle(ListAllItems request, CancellationToken cancellationToken)
            => Task.FromResult(_read.ListAll());

        public Task<TodoLists.List> Handle(GetTodoItemById request, CancellationToken cancellationToken)
            => Task.FromResult(_read.GetByAggregateId(request.AggregateId));
    }
}