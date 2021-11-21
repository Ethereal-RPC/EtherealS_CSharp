namespace EtherealS.Core.Interface
{
    public interface IBaseIoc
    {
        public void RegisterIoc(string name, object instance);
        public void UnRegisterIoc(string name);
        public bool GetIocObject(string name, out object instance);
    }
}
