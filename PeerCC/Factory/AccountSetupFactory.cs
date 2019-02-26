using ClientCore.Account;
using ClientCore.Factory;
using PeerCC.Account;

namespace PeerCC.Factory
{
    public class AccountSetupFactory : IAccountSetupFactory
    {
        public IAccountSetup Create()
        {
            return new AccountSetup();
        }
    }
}
