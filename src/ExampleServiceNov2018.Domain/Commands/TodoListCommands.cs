namespace ExampleServiceNov2018.Domain.Commands
{
    public abstract class Command
    {
        public string AggregateId { get; set; }
    }

    public class NameTodoList : Command
    {
        public string Name { get; set; }
    }

    public class AddTodoItem : Command
    {
        public int ItemNumber { get; set; }
        public string ItemText { get; set; }
    }

    public class CheckTodoItem : Command
    {
        public int ItemNumber { get; set; }
    }

    public class UncheckTodoItem : Command
    {
        public int ItemNumber { get; set; }
    }
}