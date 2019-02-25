namespace ClientCore.Signaling
{
    public enum MessageEnhancedType
    {
        Offer,
        Answer,
        Hangup,
        Text,
        Other,
    }

    public enum DeliveryMethod
    {
        Server,
        P2P,
        Both
    }

    public class EnhancedMessage
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
        public MessageEnhancedType Type;
        /// <summary>
        /// Get or set the message type when "other" is used.
        /// </summary>
        public string TypeOther;
        /// <summary>
        /// Get or set the encoding format of the media description.
        /// </summary>
        public MediaDescriptionEncodingType MediaEncoding;
        /// <summary>
        /// Get or set how the media should be delivered to the recipient.
        /// </summary>
        public DeliveryMethod DeliveryMethod;
        /// <summary>
        /// Get or set the associated content related to the message type.
        /// </summary>
        public string Content;

        public static EnhancedMessage FromMessage(Message message)
        {
            var result = new EnhancedMessage();
            return result;
        }
        public Message ToMessage()
        {
            var result = new Message();
            return result;
        }
    }
}
