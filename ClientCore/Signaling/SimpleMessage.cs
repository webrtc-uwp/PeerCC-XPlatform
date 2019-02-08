using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.Signaling
{
    public enum SimpleMessageType
    {
        Offer,
        Answer,
        Bye
    }

    public class SimpleMessage
    {
        /// <summary>
        /// Get or set the unique message identifier.
        /// </summary>
        public string Id;
        /// <summary>
        /// Get or set the peer identifier for the indented recipient.
        /// </summary>
        public string PeerId;
        /// <summary>
        /// Get or set the message type being delivered.
        /// </summary>
        public SimpleMessageType Type;
        /// <summary>
        /// Get or set the encoding format of the media description.
        /// </summary>
        public MediaDescriptionEncodingType MediaEncoding;
        /// <summary>
        /// Get or set the associated content related to the message type.
        /// </summary>
        public string Content;

        public static SimpleMessage FromMessage(Message message)
        {
            var result = new SimpleMessage();
            return result;
        }
        public Message ToMessage()
        {
            var result = new Message();
            return result;
        }
    }
}
