using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Manager.AbstractType
{
    public class AbstractTypeManager
    {

        public Dictionary<Type, AbstractType> AbstractTypesByType { get; set; } = new Dictionary<Type, AbstractType>();
        public Dictionary<string, AbstractType> AbstractTypesByName { get; set; } = new Dictionary<string, AbstractType>();

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
                AbstractTypesByName.Add(type.Name, type);
                if (!AbstractTypesByType.ContainsKey(type.Type)) AbstractTypesByType.Add(type.Type, type);
            }
            catch (Exception)
            {
                if (AbstractTypesByName.ContainsKey(type.Name) || AbstractTypesByType.ContainsKey(type.Type)) Console.WriteLine($"注册类型:{type.Type}转{type.Name}发生异常");
            }
        }

        public bool Get(string name, out AbstractType type)
        {
            return AbstractTypesByName.TryGetValue(name, out type);
        }

        public bool Get(Type type, out AbstractType abstractType)
        {
            return AbstractTypesByType.TryGetValue(type, out abstractType);
        }

        internal bool Get(ParameterInfo parameterInfo, out AbstractType type)
        {
            ParamAttribute paramAttribute = parameterInfo.GetCustomAttribute<ParamAttribute>();
            if (paramAttribute != null)
            {
                return AbstractTypesByName.TryGetValue(paramAttribute.Type, out type);
            }
            return AbstractTypesByType.TryGetValue(parameterInfo.ParameterType, out type);
        }
        internal bool Get(string name, Type type, out AbstractType abstractType)
        {
            if (name != null)
            {
                return AbstractTypesByName.TryGetValue(name, out abstractType);
            }
            return AbstractTypesByType.TryGetValue(type, out abstractType);
        }
    }
}
