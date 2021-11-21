using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EtherealS.Core.Model
{
    /// <summary>
    /// RPCType配置项
    /// </summary>
    public class AbstractTypes
    {
        /// <summary>
        /// 从Type到RPCType的映射表
        /// </summary>
        public Dictionary<Type, AbstractType> TypesByType { get; set; } = new Dictionary<Type, AbstractType>();
        /// <summary>
        /// 从Name到RPCTYpe的映射表
        /// </summary>
        public Dictionary<string, AbstractType> TypesByName { get; set; } = new Dictionary<string, AbstractType>();

        public AbstractTypes()
        {

        }
        public void Add<T>(string typeName)
        {
            Add<T>(typeName, obj => JsonConvert.SerializeObject(obj), obj => JsonConvert.DeserializeObject<T>(obj));
        }
        public void Add<T>(string typeName, AbstractType.SerializeDelegage serializDelegage, AbstractType.DeserializeDelegage deserializeDelegage)
        {
            AbstractType type = new AbstractType();
            type.Name = typeName;
            type.Type = typeof(T);
            if (serializDelegage == null) type.Serialize = obj => JsonConvert.SerializeObject(obj);
            else type.Serialize = serializDelegage;
            if (deserializeDelegage == null) type.Deserialize = obj => JsonConvert.DeserializeObject<T>(obj);
            else type.Deserialize = deserializeDelegage;
            Add(type);
        }
        /// <summary>
        /// 注册RPCType
        /// </summary>
        /// <param name="type">RPCType</param>
        public void Add(AbstractType type)
        {
            try
            {
                TypesByName.Add(type.Name, type);
                TypesByType.Add(type.Type, type);
            }
            catch (Exception)
            {
                if (TypesByName.ContainsKey(type.Name) || TypesByType.ContainsKey(type.Type)) Console.WriteLine($"注册类型:{type.Type}转{type.Name}发生异常");
            }
        }
    }
}
