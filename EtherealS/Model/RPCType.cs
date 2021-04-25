using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS.Model
{
    public class RPCType
    {
        #region --委托--
        public delegate object DeserializeDelegage(string obj);
        public delegate string SerializeDelegage(object obj);
        #endregion

        #region --字段--
        private DeserializeDelegage deserialize;
        private SerializeDelegage serialize;
        private Type type;
        private string name;
        #endregion

        #region --属性--
        public DeserializeDelegage Deserialize { get => deserialize; set => deserialize = value; }
        public SerializeDelegage Serialize { get => serialize; set => serialize = value; }
        public Type Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }

        #endregion
    }
}
