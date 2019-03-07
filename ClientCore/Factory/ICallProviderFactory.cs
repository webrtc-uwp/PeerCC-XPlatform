using ClientCore.Call;

namespace ClientCore.Factory
{
    public interface ICallProviderFactory
    {
        ICallProvider Create();
    }
}
