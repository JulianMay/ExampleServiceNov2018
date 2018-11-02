using System;
using System.Collections.Generic;
using System.Linq;

namespace ExampleServiceNov2018.Domain
{
    /// <summary>
    /// Consider making an immutable version?
    /// </summary>
    public abstract class AggregateState
    {
        public readonly string Id;
        public readonly int LoadedRevision;
        private readonly List<object> _uncomittedEvents = new List<object>();
        public IReadOnlyCollection<object> UncommittedEvents => _uncomittedEvents; 


        protected AggregateState(string id, int loadedRevision)
        {
            Id = id;
            LoadedRevision = loadedRevision;
        }

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