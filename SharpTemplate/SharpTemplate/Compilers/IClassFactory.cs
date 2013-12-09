namespace SharpTemplate.Compilers
{
    public interface IClassFactory
    {
        TData CreateInstance<TData>(string type, params object[] pars);
    }
}
