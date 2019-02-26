using ClientCore.Contacts;
using System;

namespace PeerCC.Contacts
{
    public class ContactDisposition : IContactDisposition
    {
        public Disposition Disposition => throw new NotImplementedException();

        public IContact Contact => throw new NotImplementedException();
    }
}
