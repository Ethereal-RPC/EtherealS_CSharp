using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EtherealS.Model
{
    public class RPCType
    {
        public delegate object ConvertDelegage(string obj);
        public Dictionary<Type, string> AbstractName { get; set; } = new Dictionary<Type, string>();
        public Dictionary<string, Type> AbstractType { get; set; } = new Dictionary<string,Type>();
        public Dictionary<string, ConvertDelegage> TypeConvert { get; set; } = new Dictionary<string, ConvertDelegage>();

        public RPCType()
        {

        }
        public void Add<T>(string typeName)
        {
            try
            {
                AbstractName.Add(typeof(T), typeName);
                TypeConvert.Add(typeName, obj=>JsonConvert.DeserializeObject<T>(obj));
                AbstractType.Add(typeName, typeof(T));
            }
            catch (Exception)
            {
                if (TypeConvert.ContainsKey(typeName) || AbstractName.ContainsKey(typeof(T))) Console.WriteLine($"注册类型:{typeof(T)}转{typeName}发生异常");
            }
        }
        public void Add<T>(string typeName, ConvertDelegage convert)
        {
            try
            {
                AbstractName.Add(typeof(T), typeName);
                TypeConvert.Add(typeName, convert);
                AbstractType.Add(typeName, typeof(T));
            }
            catch (Exception)
            {
                if ( TypeConvert.ContainsKey(typeName) || AbstractName.ContainsKey(typeof(T))) Console.WriteLine($"注册类型:{typeof(T)}转{typeName}发生异常");
            }
        }
    }
}
