using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EtherealS.Model
{
    public class RPCTypeConfig
    {

        public Dictionary<Type, RPCType> TypesByType { get; set; } = new Dictionary<Type, RPCType>();
        public Dictionary<string, RPCType> TypesByName { get; set; } = new Dictionary<string, RPCType>();

        public RPCTypeConfig()
        {

        }
        public void Add<T>(string typeName)
        {
            try
            {
                RPCType type = new RPCType();
                
                type.Type = typeof(T);
                type.Serialize = obj => JsonConvert.SerializeObject(obj);
                type.Deserialize = obj => JsonConvert.DeserializeObject<T>(obj);
                TypesByName.Add(typeName, type);
                TypesByType.Add(typeof(T), type);
            }
            catch (Exception)
            {
                if (TypesByName.ContainsKey(typeName) || TypesByType.ContainsKey(typeof(T))) Console.WriteLine($"注册类型:{typeof(T)}转{typeName}发生异常");
            }
        }
        public void Add<T>(string typeName, RPCType.SerializeDelegage serializDelegage, RPCType.DeserializeDelegage deserializeDelegage)
        {
            try
            {
                RPCType type = new RPCType();
                if (serializDelegage == null) type.Serialize = obj => JsonConvert.SerializeObject(obj);
                else type.Serialize = serializDelegage;
                if (deserializeDelegage == null) type.Deserialize = obj => JsonConvert.DeserializeObject<T>(obj);
                else type.Deserialize = deserializeDelegage;
                TypesByName.Add(typeName, type);
                TypesByType.Add(typeof(T), type);
            }
            catch (Exception)
            {
                if (TypesByName.ContainsKey(typeName) || TypesByType.ContainsKey(typeof(T))) Console.WriteLine($"注册类型:{typeof(T)}转{typeName}发生异常");
            }
        }
    }
}
