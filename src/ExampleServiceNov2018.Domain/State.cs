using System.Collections.Generic;

namespace ExampleServiceNov2018.Domain
{
    /// <summary>
    ///     Consider making an immutable version?
    /// </summary>
    public abstract class AggregateState
    {
        private readonly List<object> _uncomittedEvents = new List<object>();
        public readonly string Id;
        public readonly int LoadedRevision;


        protected AggregateState(string id, int loadedRevision)
        {
            Id = id;
            LoadedRevision = loadedRevision;
        }

        public IReadOnlyCollection<object> UncommittedEvents => _uncomittedEvents;

        public void Emit(object @event)
        {
            Apply(@event);
            _uncomittedEvents.Add(@event);
        }

        public abstract void Apply(object @event);
    }

    public abstract class ValueObject<T>
    {
        public abstract T Apply(object @event);
    }
}