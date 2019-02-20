using ClientCore.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.PeerCCContactImpl
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
