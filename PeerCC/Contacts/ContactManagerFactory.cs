using ClientCore.Contacts;

namespace PeerCC.Contacts
{
    public class ContactManagerFactory
    {
        public static IContactManager Create()
        {
            return new ContactManager();
        }
    }
}
