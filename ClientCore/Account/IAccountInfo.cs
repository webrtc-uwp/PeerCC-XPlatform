using ClientCore.Contacts;
using ClientCore.Signaling;

namespace ClientCore.Account
{
    public interface IAccountInfo
    {
        /// <summary>
        /// Gets the signaler associated with the account.
        /// </summary>
        ISignaller Signaler { get; }
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
    }
}
