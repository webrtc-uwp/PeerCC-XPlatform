using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.Signaling
{
    public class Message
    {
        /// <summary>
        /// Get or set a unique identifier for the message.
        /// </summary>
        public string Id;
        /// <summary>
        /// Get or set the peer identifier of which peer to deliver the
        /// message.
        /// </summary>
        public string PeerId;
        /// <summary>
        /// Get or set the content delivered to the peer.
        /// </summary>
        public string Content;
    }
}
