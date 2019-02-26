using ClientCore.Account;
using System;
using System.Threading.Tasks;

namespace PeerCC.Account
{
    public class AccountSetup : IAccountSetup
    {
        public static AccountSetup Create()
        {
            return new AccountSetup();
        }

        public Task<IAccount> GetLoginInfoAsync(string loginCompletePayload)
        {
            throw new NotImplementedException();
        }

        public Task<string> LoginAysnc(LoginInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
