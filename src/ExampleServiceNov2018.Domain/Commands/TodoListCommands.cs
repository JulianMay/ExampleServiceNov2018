namespace ExampleServiceNov2018.Domain.Commands
{
    public abstract class Command
    {
        public string AggregateId;
    }
    
    public class NameTodoList : Command
    {
        public string Name;
    }
    
    public class AddTodoItem : Command
    {     
        public int ItemNumber;
        public string ItemText;
    }

    public class CheckTodoItem : Command
    {
        public int ItemNumber;
    }
    
    public class UncheckTodoItem : Command
    {
        public int ItemNumber;
    }
}