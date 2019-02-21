using ClientCore.Account;
using ClientCore.Factory;
using ClientCore.PeerCCImpl.Account;

namespace ClientCore.PeerCCImpl.Account
{
    public class AccountSetupFactory : IAccountSetupFactory
    {
        IAccountSetup IAccountSetupFactory.Create()
        {
            return AccountSetup.Create();
        }
    }
}
