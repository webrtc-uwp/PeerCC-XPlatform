using ClientCore.Account;
using ClientCore.Factory;
using ClientCore.PeerCCLoginImpl;

namespace ClientCore.PeerCCImpl.Factory
{
    public class AccountSetupFactory : IAccountSetupFactory
    {
        IAccountSetup Create()
        {
            return new AccountSetup();
        }
    }
}
