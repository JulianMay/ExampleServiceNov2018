using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlStreamStore.Streams;

namespace ExampleServiceNov2018.ReadService
{
    internal class Deserialization
    {
        //DUPLICATION FROM TodoListRepository ( Infrastructure / ReadService could share dependency, but should they? )
        public static async Task<object> Deserialize(StreamMessage message)
        {
            if(!_typeCache.TryGetValue(message.Type, out var eventType))
            {
                //Guess type by convention:
                var typename = $"ExampleServiceNov2018.Domain.Events.{message.Type}, ExampleServiceNov2018.Domain";
                
                eventType = Type.GetType(typename);
                _typeCache[message.Type] = eventType;
            }

            var json = await message.GetJsonData();

            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, eventType);

            //SimpleJson throws NRE on deserialization!
            //NullReferenceException: Object reference not set to an instance of an object.
            //  StreamStoreStore.Json.PocoJsonSerializerStrategy.DeserializeObject(object value, Type type)
            //  StreamStoreStore.Json.SimpleJson.DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
            //  StreamStoreStore.Json.SimpleJson.DeserializeObject(string json, Type type)


        }
        
        private static Dictionary<string,Type> _typeCache = new Dictionary<string, Type>();
    }
}