using EtherealS.Core.Manager.Event;
using EtherealS.Core.Model;
using System.Collections.Generic;

namespace EtherealS.Core.Manager.Ioc
{
    public class IocManager
    {
        private Dictionary<string, object> IocContainer { get; set; } = new();
        internal EventManager EventManager { get; set; } = new();

        public void Register(string name, object instance)
        {
            if (IocContainer.ContainsKey(name))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{name}实例已注册");
            }
            IocContainer.Add(name, instance);
            EventManager.RegisterEventMethod(name, instance);
        }
        public void UnRegister(string name)
        {
            if (IocContainer.TryGetValue(name, out object instance))
            {
                IocContainer.Remove(name);
                EventManager.UnRegisterEventMethod(name, instance);
            }
        }
        public bool Get(string name, out object instance)
        {
            return IocContainer.TryGetValue(name, out instance);
        }
        public object Get(string name)
        {
            IocContainer.TryGetValue(name, out object instance);
            return instance;
        }
    }
}
