using MediatR;

namespace ExampleServiceNov2018.ReadService
{
    public class TodoListCollectionDTO
    {
        public TodoListDTO[] Collection { get; set; }
    }

    public class TodoListDTO
    {
        public string AggregateId { get; set; }
        public string Name { get; set; }
        public TodoItemDTO[] Items { get; set; }
    }

    public class TodoItemDTO
    {
        public string Text { get; set; }
        public bool Checked { get; set; }
    }

    public class ListAllItems : IRequest<TodoListCollectionDTO>
    {
    }

    public class GetTodoListById : IRequest<TodoListDTO>
    {
        public string AggregateId;
    }
}