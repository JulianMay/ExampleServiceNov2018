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
        public List<object> UncommittedEvents = new List<object>();


        protected AggregateState(string id, int loadedRevision)
        {
            Id = id;
            LoadedRevision = loadedRevision;
        }

        public void Emit(object @event)
        {
            Apply(@event);
            UncommittedEvents.Add(@event);
        }
        
        public abstract void Apply(object @event);
    }

    public abstract class ValueObject<T>
    {
        public abstract T Apply(object @event);
    }
}