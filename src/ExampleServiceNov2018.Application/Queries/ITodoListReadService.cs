namespace ExampleServiceNov2018.Application.Queries
{
    public interface ITodoListReadService
    {
        TodoLists ListAll();
        TodoLists.List GetByAggregateId(string aggregateId);
    }
}