using ClientCore.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.PeerCCContactImpl
{
    public class ContactManagerFactory
    {
        public static IContactManager Create()
        {
            return new ContactManager();
        }
    }
}
