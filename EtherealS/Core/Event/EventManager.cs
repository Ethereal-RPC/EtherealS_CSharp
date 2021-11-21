using EtherealS.Core.Event.Attribute;
using EtherealS.Core.Model;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.Core.Event
{
    public class EventManager
    {
        public Dictionary<(string, string), MethodInfo> MethodEvents { get; set; } = new Dictionary<(string, string), MethodInfo>();

        internal void InvokeEvent(object instance, EventSender requestEvent, Dictionary<string, object> @params, EventContext context)
        {
            if (!MethodEvents.TryGetValue((requestEvent.InstanceName, requestEvent.Mapping), out MethodInfo method))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{requestEvent.InstanceName}-{requestEvent.Mapping}中{requestEvent.Mapping}方法不存在");
            }
            ParameterInfo[] parameterInfos = method.GetParameters();
            object[] eventParams = new object[parameterInfos.Length];
            for (int i = 0; i < eventParams.Length; i++)
            {
                EventContextParam context_attribute = parameterInfos[i].GetCustomAttribute<EventContextParam>();
                if (context_attribute != null)
                {
                    eventParams[i] = context;
                }
                else if (requestEvent.paramsMapping.TryGetValue(parameterInfos[i].Name, out string param_name))
                {
                    if (!@params.TryGetValue(param_name, out object param))
                    {
                        throw new TrackException(TrackException.ErrorCode.Runtime, $"{context.Method.Name}调用{requestEvent.InstanceName}实例的{requestEvent.Mapping}事件方法时，方法未提供{parameterInfos[i].Name}-{param_name}参数映射");
                    }
                    eventParams[i] = param;
                }
                else throw new TrackException(TrackException.ErrorCode.Runtime, $"{context.Method.Name}调用{requestEvent.InstanceName}实例的{requestEvent.Mapping}事件方法时，未定义{parameterInfos[i].Name}参数映射");
            }
            method.Invoke(instance, eventParams);
        }
        public void RegisterEventMethod(string name, object instance)
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.Event attribute = method.GetCustomAttribute<Attribute.Event>();
                if (attribute != null)
                {
                    MethodEvents.Add((name, attribute.Mapping), method);
                }
            }
        }
        public void UnRegisterEventMethod(string name, object instance)
        {
            foreach (MethodInfo method in instance.GetType().GetMethods())
            {
                Attribute.Event attribute = method.GetCustomAttribute<Attribute.Event>();
                if (attribute != null)
                {
                    MethodEvents.Remove((name, attribute.Mapping));
                }
            }
        }
    }
}
