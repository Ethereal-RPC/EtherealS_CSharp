namespace EtherealS.Extension.Authority
{
    public interface IAuthorityCheck:IAuthoritable
    {
        public bool Check(IAuthoritable authoritable);
    }
}
