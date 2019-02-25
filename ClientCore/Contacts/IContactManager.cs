using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientCore.Contacts
{
    public interface IContactManager
    {
        /// <summary>
        /// Obtain a list of contacts that have changes since the last call
        /// to this method. This method will not return if there are no
        /// contact changes available and will throw an exception should
        /// access to the source contact list be no longer available.
        /// </summary>
        /// <returns>A list of changed contacts.</returns>
        Task<IList<IContactDisposition>> GetContactsChangesAsync();
    }
}
