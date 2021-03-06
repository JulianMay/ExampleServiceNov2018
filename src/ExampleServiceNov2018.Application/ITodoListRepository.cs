﻿using System.Threading.Tasks;
using ExampleServiceNov2018.Domain.TodoList;

namespace ExampleServiceNov2018.Application
{
    public interface ITodoListRepository
    {
        Task<TodoListState> Load(string aggregateId);
        Task<int> Save(TodoListState aggregate);
    }
}