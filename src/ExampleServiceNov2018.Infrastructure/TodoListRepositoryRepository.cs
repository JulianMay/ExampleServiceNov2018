using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleServiceNov2018.Application;
using ExampleServiceNov2018.Domain.TodoList;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using StreamStoreStore.Json;

namespace ExampleServiceNov2018.Infrastructure
{
    public class TodoListRepositoryRepository : ITodoListRepository
    {
        private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
        private readonly IStreamStore _store;

        public TodoListRepositoryRepository(IStreamStore store)
        {
            _store = store;
        }

        public async Task<TodoListState> Load(string aggregateId)
        {
            var stream = await _store.ReadStreamForwards(new StreamId(aggregateId), 0, int.MaxValue, true);
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
                await _store.AppendToStream(aggregate.Id, aggregate.LoadedRevision, toAppend);
            return writeResult.CurrentVersion;
        }

        private NewStreamMessage Serialize(object @event)
        {
            return new NewStreamMessage(
                Guid.NewGuid(),
                @event.GetType().Name,
                SimpleJson.SerializeObject(@event));
        }


        private async Task<object> Deserialize(StreamMessage message)
        {
            if (!_typeCache.TryGetValue(message.Type, out var eventType))
            {
                //Guess type by convention:
                var typename = $"ExampleServiceNov2018.Domain.Events.{message.Type}, ExampleServiceNov2018.Domain";

                eventType = Type.GetType(typename);
                _typeCache[message.Type] = eventType;
            }

            var json = await message.GetJsonData();

            return JsonConvert.DeserializeObject(json, eventType);

            //SimpleJson throws NRE on deserialization!
            //NullReferenceException: Object reference not set to an instance of an object.
            //  StreamStoreStore.Json.PocoJsonSerializerStrategy.DeserializeObject(object value, Type type)
            //  StreamStoreStore.Json.SimpleJson.DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
            //  StreamStoreStore.Json.SimpleJson.DeserializeObject(string json, Type type)
        }
    }
}