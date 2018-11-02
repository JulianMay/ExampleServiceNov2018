using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExampleServiceNov2018.Application;
using ExampleServiceNov2018.Domain;
using SqlStreamStore;
using SqlStreamStore.Streams;
using StreamStoreStore.Json;

namespace ExampleServiceNov2018.Infrastructure
{
    public class TodoListRepository : ITodoLists
    {
        private readonly IStreamStore _store;
        private readonly CancellationToken _cancellationToken;

        public TodoListRepository(IStreamStore store, CancellationToken cancellationToken)
        {
            _store = store;
            _cancellationToken = cancellationToken;
        }

        public async Task<TodoListState> Load(string aggregateId)
        {            
            var stream = await _store.ReadStreamForwards(new StreamId(aggregateId), 0, int.MaxValue, true, _cancellationToken);
            var aggregate = new TodoListState(aggregateId, stream.LastStreamVersion);

            foreach (var message in stream.Messages)
            {
                var @event = await Deserialize(message);
                aggregate.Apply(@event);
            }

            return aggregate;
        }

        public async Task<int> Save(TodoListState aggregate)
        {
            var toAppend = aggregate.UncommittedEvents.Select(Serialize).ToArray();
            var writeResult =
                await _store.AppendToStream(aggregate.Id, aggregate.LoadedRevision, toAppend, _cancellationToken);
            return writeResult.CurrentVersion;
        }

        private NewStreamMessage Serialize(object @event)
            => new NewStreamMessage(
                    Guid.NewGuid(),
                    @event.GetType().AssemblyQualifiedName,
                    SimpleJson.SerializeObject(@event)); //metadata not yet added
    
        
        private async Task<object> Deserialize(StreamMessage message)
        {
            if(!_typeCache.TryGetValue(message.Type, out var eventType))
            {
                eventType = Type.GetType(message.Type);
                _typeCache[message.Type] = eventType;
            }

            var json = await message.GetJsonData();
            return SimpleJson.DeserializeObject(json, eventType);
        }
        
        
        private static Dictionary<string,Type> _typeCache = new Dictionary<string, Type>();
    }
}
