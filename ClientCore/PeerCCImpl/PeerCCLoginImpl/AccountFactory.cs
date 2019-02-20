using ClientCore.Account;
using System.Threading.Tasks;

namespace ClientCore.PeerCCLoginImpl
{
    public class AccountFactory
    {
        static Task<IAccountSetup> GetAccountAsync()
        {
            return Task.Run(() => (IAccountSetup)new AccountSetup());
        }
    }
}
