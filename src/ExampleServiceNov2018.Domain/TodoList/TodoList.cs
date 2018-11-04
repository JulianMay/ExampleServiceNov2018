using System;
using ExampleServiceNov2018.Domain.Commands;
using ExampleServiceNov2018.Domain.Events;

namespace ExampleServiceNov2018.Domain.TodoList
{
    /// <summary>
    /// This is invariant-checking, notice state is method-injected (as can dependencies)
    /// We expect command to have been structurally validated at this point, and state's id to match the commands
    /// </summary>
    public static class TodoList
    {
        public static TodoListState Write(object command, TodoListState todoList)
        {                        
            switch (command)
            {
                case NameTodoList c:     
                    todoList = Transition(todoList,new TodoListNamed(c.AggregateId, c.Name)); 
                    break;
                
                case AddTodoItem c:    
                    if(todoList.Items.ContainsKey(c.ItemNumber))
                        throw new ArgumentException("This list already has an item with that number");                   
                    todoList = Transition(todoList,new TodoItemAdded(c.AggregateId, c.ItemText, c.ItemNumber));
                    break;
                
                case CheckTodoItem c:    
                    todoList = Transition(todoList,new TodoItemChecked(c.AggregateId, c.ItemNumber));
                    break;
                
                case UncheckTodoItem c:  
                    todoList = Transition(todoList,new TodoItemUnchecked(c.AggregateId, c.ItemNumber));
                    break;
            }

            return todoList;
        }

        private static TodoListState Transition(TodoListState todoList,object @event)
        {
            todoList.Emit(@event);
            return todoList;
        }
        
        
    }
}
