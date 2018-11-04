using System.Collections.Generic;
using ExampleServiceNov2018.Domain.Events;

namespace ExampleServiceNov2018.Domain.TodoList
{
    public class TodoListState : AggregateState
    {
//        //A full constructor could load from snapshot
//        public TodoListState(int loadedRevision, string name, Dictionary<int, TodoItem> items) : base(loadedRevision)
//        {
//            Name = name;
//            _items = items;
//        }

        public TodoListState(string id, int loadedRevision) : base(id, loadedRevision)
        {
            Name = string.Empty;
            _items = new Dictionary<int, TodoItem>();
        }
        
        

        public string Name { get; private set; }

        private Dictionary<int, TodoItem> _items;
        public IReadOnlyDictionary<int, TodoItem> Items => _items;

        public override void Apply(object @event)
        {
            switch (@event)
            {
                case TodoListNamed listNamed: Name = listNamed.Name; break;
                case TodoItemAdded itemAdded: UpdateItem(itemAdded.Number,itemAdded); break;
                case TodoItemChecked chkd: UpdateItem(chkd.Number,chkd); break;
                case TodoItemUnchecked unchkd: UpdateItem(unchkd.Number,unchkd); break;
            }
        }

        private void UpdateItem(int number, object @event)
        {
            var item = Items.TryGetValue(number, out var existing)
                ? existing.Apply(@event)
                : TodoItem.New.Apply(@event);
                    
            _items[number] = item;
        }

        
    }
}