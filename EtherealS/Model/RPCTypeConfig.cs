using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EtherealS.Model
{
    /// <summary>
    /// RPCType配置项
    /// </summary>
    public class RPCTypeConfig
    {
        /// <summary>
        /// 从Type到RPCType的映射表
        /// </summary>
        public Dictionary<Type, RPCType> TypesByType { get; set; } = new Dictionary<Type, RPCType>();
        /// <summary>
        /// 从Name到RPCTYpe的映射表
        /// </summary>
        public Dictionary<string, RPCType> TypesByName { get; set; } = new Dictionary<string, RPCType>();

        public RPCTypeConfig()
        {

        }
        /// <summary>
        /// 注册RPCType
        /// </summary>
        /// <typeparam name="T">映射类</typeparam>
        /// <param name="typeName">映射名</param>
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
        /// <summary>
        /// 注册RPCType
        /// </summary>
        /// <typeparam name="T">映射类</typeparam>
        /// <param name="typeName">映射名</param>
        /// <param name="serializDelegage">序列化委托实现</param>
        /// <param name="deserializeDelegage">逆序列化委托实现</param>
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
        /// <summary>
        /// 注册RPCType
        /// </summary>
        /// <param name="type">RPCType</param>
        public void Add(RPCType type)
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
