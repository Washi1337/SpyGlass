namespace SpyGlass.Injection
{
    public interface IInjector
    {
        void InjectDll(RemoteProcess process, string pathToDll);
    }
}