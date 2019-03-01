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

        public Task<string> LoginAysnc(LoginInfo info)
        {
            return Task.Run(() => "");
        }

        public Task<IAccount> GetLoginInfoAsync(string loginCompletePayload)
        {
            throw new NotImplementedException();
        }
    }
}
