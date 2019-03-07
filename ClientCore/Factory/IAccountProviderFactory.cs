using ClientCore.Account;

namespace ClientCore.Factory
{
    public interface IAccountProviderFactory
    {
        IAccountProvider Create();
    }
}
