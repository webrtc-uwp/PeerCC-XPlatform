using ClientCore.Account;
using ClientCore.Factory;

namespace PeerCC.Account
{
    public class AccountSetupFactory : IAccountSetupFactory
    {
        IAccountSetup IAccountSetupFactory.Create()
        {
            return AccountSetup.Create();
        }
    }
}
