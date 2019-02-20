using ClientCore.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.PeerCCLoginImpl
{
    public class AccountSetup : IAccountSetup
    {
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
