using System.Threading.Tasks;

namespace ClientCore.Account
{
    public interface IAccountProvider
    {
        /// <summary>
        /// Obtains a URL to display inside a browser control in order to
        /// complete the login process.
        /// </summary>
        /// <returns>A URL to display in a browser control or "" if no browser
        /// window is required to be displayed.</returns>
        Task<string> LoginAysnc(LoginInfo info);

        /// <summary>
        /// Once the login is complete this method can be called to obtain
        /// information about the logged in identity given a payload passed
        /// to the build in JavaScript complete routine. This method will
        /// throw an exception if the login could not be completed.
        /// </summary>
        /// <param name="loginCompletePayload">The payload as sent to the
        /// built-in JavaScript login complete routine. Only
        /// the payload sent to the bound JavaScript method maybe
        /// used with this method or "" may be passed in if ""
        /// login URL was returned from the LoginAsync method.</param>
        /// <returns>Returns the login information associated with the
        /// logged in account.</returns>
        Task<IAccount> GetLoginInfoAsync(string loginCompletePayload);
    }
}
