using ClientCore.Account;
using PeerCC.Signaling;
using System;
using System.Threading.Tasks;

namespace PeerCC.Account
{
    public class AccountProvider : IAccountProvider
    {
        public static AccountProvider Create()
        {
            return new AccountProvider();
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
