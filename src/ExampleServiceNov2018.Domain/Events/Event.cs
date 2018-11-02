namespace ExampleServiceNov2018.Domain.Events
{
    public abstract class Event
    {
        public readonly string AggregateId;

        protected Event(string aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}