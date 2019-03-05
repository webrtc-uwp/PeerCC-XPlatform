using ClientCore.Account;
using PeerCC.Signaling;
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

        public Account GetSignaler(string ip, int port, HttpSignaler httpSignaler)
        {
            Account account = new Account();
            account.SetSelfIdentityUri($"{ip}:{port}");
            account.SetSignaler(new HttpSignaler());

            return account;
        }
    }
}
