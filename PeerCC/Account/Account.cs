using ClientCore.Account;
using ClientCore.Contacts;
using ClientCore.Signaling;
using System;
using System.Threading.Tasks;

namespace PeerCC.Account
{
    public class Account : IAccount
    {
        public string SelfIdentityUri => throw new NotImplementedException();

        public ISignaler Signaler => throw new NotImplementedException();

        public IContactManager ContactManager => throw new NotImplementedException();

        public string CachedCredentialsPayload => throw new NotImplementedException();

        public Task Logout()
        {
            throw new NotImplementedException();
        }
    }
}
