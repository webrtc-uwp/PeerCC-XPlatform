using System.Collections.Generic;

namespace ClientCore.Call
{
    public class IceServer
    {
        /// <summary>
        /// Get or set the associated list of STUN or TURN server URIs.
        /// </summary>
        public List<string> Urls;
        /// <summary>
        /// Get or set the associated login username (if TURN).
        /// </summary>
        public string Username;
        /// <summary>
        /// Get or set the associated login credential (if TURN).
        /// </summary>
        public string Credential;
    }
}
