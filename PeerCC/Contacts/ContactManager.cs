using ClientCore.Contacts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerCC.Contacts
{
    public class ContactManager : IContactManager
    {
        public Task<IList<IContactDisposition>> GetContactsChangesAsync()
        {
            return Task.Run(() => contactDispositionList);
        }

        IList<IContactDisposition> contactDispositionList = new List<IContactDisposition>();

        void AddContactDisposition()
        {
            contactDispositionList.Add(new ContactDisposition());
        }
    }
}
