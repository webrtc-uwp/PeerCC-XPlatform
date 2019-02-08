using ClientCore.Contacts;
using ClientCore.Signaling;
using System.Threading.Tasks;

namespace ClientCore.Account
{
    public interface IAccount
    {
        /// <summary>
        /// Gets the identity URI for the "self" associated to this contact
        /// manager.
        /// </summary>
        string SelfIdentityUri { get; }

        /// <summary>
        /// Gets the signaler associated with the account.
        /// </summary>
        ISignaler Signaler { get; }
        /// <summary>
        /// Gets the contact manager used to manage contacts for the
        /// logged-in identity.
        /// </summary>
        IContactManager ContactManager { get; }
        /// <summary>
        /// Gets a set of cached credential that can be used to login the
        /// identity again (perhaps without even needing to display the
        /// browser window to the end user).
        /// </summary>
        string CachedCredentialsPayload { get; }

        /// <summary>
        /// Logout of the logged in identity.
        /// </summary>
        /// <returns>Returns when logged out.</returns>
        Task Logout();
    }
}
