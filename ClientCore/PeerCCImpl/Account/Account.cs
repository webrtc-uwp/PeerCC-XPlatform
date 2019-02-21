using ClientCore.Account;
using ClientCore.Contacts;
using ClientCore.Signaling;
using System;
using System.Threading.Tasks;

namespace ClientCore.PeerCCImpl.Account
{
    public class Account : IAccount
    {
        public string SelfIdentityUri => throw new NotImplementedException();

        public ISignaler Signaler => SignalerFactory.Create();

        public IContactManager ContactManager => ContactManagerFactory.Create();

        public string CachedCredentialsPayload => throw new NotImplementedException();

        public Task Logout()
        {
            throw new NotImplementedException();
        }
    }
}
