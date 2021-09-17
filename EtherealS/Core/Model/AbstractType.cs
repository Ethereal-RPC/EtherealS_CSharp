using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS.Core.Model
{
    /// <summary>
    /// 中间层抽象数据类
    /// </summary>
    public class AbstractType
    {
        #region --委托--
        /// <summary>
        /// 序列化委托
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <returns></returns>
        public delegate object DeserializeDelegage(string obj);
        /// <summary>
        /// 反序列化委托
        /// </summary>
        /// <param name="obj">反序列化文本</param>
        /// <returns></returns>
        public delegate string SerializeDelegage(object obj);
        #endregion

        #region --字段--
        /// <summary>
        /// 反序列化委托实现
        /// </summary>
        private DeserializeDelegage deserialize;
        /// <summary>
        /// 序列化委托实现
        /// </summary>
        private SerializeDelegage serialize;
        /// <summary>
        /// 映射类
        /// </summary>
        private Type type;
        /// <summary>
        /// 映射名
        /// </summary>
        private string name;
        #endregion

        #region --属性--
        public DeserializeDelegage Deserialize { get => deserialize; set => deserialize = value; }
        public SerializeDelegage Serialize { get => serialize; set => serialize = value; }
        public Type Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }

        #endregion

        #region --构造方法--
        public AbstractType()
        {

        }

        public AbstractType(Type type,string name)
        {
            this.type = type;
            this.name = name;
        }
        #endregion
    }
}
