namespace ClientCore.Signaling
{
    public enum MediaDescriptionEncodingType
    {
        /// <summary>
        /// Media encoding is done via SDP.
        /// </summary>
        Sdp,
        /// <summary>
        /// Media encoding is done via a JSON description object (ORTC only).
        /// </summary>
        Json,
    }
}
