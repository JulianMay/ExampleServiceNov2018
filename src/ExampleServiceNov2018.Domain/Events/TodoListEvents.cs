namespace ExampleServiceNov2018.Domain.Events
{
    public class TodoListNamed : Event
    {
        public readonly string Name;

        public TodoListNamed(string aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }

    public class TodoItemAdded : Event
    {
        public readonly int Number;
        public readonly string Text;

        public TodoItemAdded(string aggregateId, string text, int number) : base(aggregateId)
        {
            Text = text;
            Number = number;
        }
    }

    public class TodoItemChecked : Event
    {
        public readonly int Number;

        public TodoItemChecked(string aggregateId, int number) : base(aggregateId)
        {
            Number = number;
        }
    }

    public class TodoItemUnchecked : Event
    {
        public readonly int Number;

        public TodoItemUnchecked(string aggregateId, int number) : base(aggregateId)
        {
            Number = number;
        }
    }
}