using ClientCore.Account;
using PeerCC.Signaling;
using System;
using System.Threading.Tasks;

namespace PeerCC.Account
{
    public class AccountProvider : IAccountProvider
    {
        public static IAccountProvider Create()
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
            account.SetSignaler(httpSignaler);

            return account;
        }

        public IAccount GetAccount()
        {
            return new Account();
        }
    }
}
