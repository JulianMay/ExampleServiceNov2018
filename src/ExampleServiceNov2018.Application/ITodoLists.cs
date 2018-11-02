using System.Threading.Tasks;
using ExampleServiceNov2018.Domain;

namespace ExampleServiceNov2018.Application
{
    public interface ITodoLists
    {
        Task<TodoListState> Load(string aggregateId);
        Task<int> Save(TodoListState aggregate);
    }
}