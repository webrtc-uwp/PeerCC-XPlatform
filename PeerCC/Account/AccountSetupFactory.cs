using ClientCore.Account;
using ClientCore.Factory;

namespace PeerCC.Account
{
    public class AccountSetupFactory : IAccountSetupFactory
    {
        IAccountProvider IAccountSetupFactory.Create()
        {
            return AccountSetup.Create();
        }
    }
}
