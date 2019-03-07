using ClientCore.Account;
using ClientCore.Factory;

namespace PeerCC.Account
{
    public class AccountProviderFactory : IAccountProviderFactory
    {
        IAccountProvider IAccountProviderFactory.Create()
        {
            return AccountProvider.Create();
        }
    }
}
