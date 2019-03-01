using ClientCore.Account;
using ClientCore.Contacts;
using ClientCore.Signaling;
using System;
using System.Threading.Tasks;

namespace PeerCC.Account
{
    public class Account : IAccount
    {
        public string SelfIdentityUri { get; private set; }

        public ISignaler Signaler { get; private set; }

        public IContactManager ContactManager { get; private set; }

        public string CachedCredentialsPayload { get; private set; }

        public Task Logout()
        {
            throw new NotImplementedException();
        }

        public void SetSelfIdentityUri(string selfIdentityUri)
        {
            SelfIdentityUri = selfIdentityUri;
        }

        public void SetSignaler(ISignaler signaler)
        {
            Signaler = signaler;
        }

        public void SetContactManager(IContactManager contactManager)
        {
            ContactManager = contactManager;
        }

        public void SetCachedCredentialsPayload(string cachedCredentialsPayload)
        {
            CachedCredentialsPayload = cachedCredentialsPayload;
        }
    }
}
