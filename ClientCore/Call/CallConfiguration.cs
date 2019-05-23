using System.Collections.Generic;

namespace ClientCore.Call
{
    public class CallConfiguration
    {
        /// <summary>
        /// Get or set the preferred audio codec id or empty string for
        /// default.
        /// </summary>
        public string PreferredAudioCodecId;
        /// <summary>
        /// Get or set the preferred video codec id or empty string for
        /// default.
        /// </summary>
        public string PreferredVideoCodecId;
        /// <summary>
        /// Get or set the preferred input audio device id or empty string for
        /// default.
        /// </summary>
        public string PreferredInputAudioDeviceId;
        /// <summary>
        /// Get or set the preferred output audio device id or empty string for
        /// default.
        /// </summary>
        public string PreferredAudioOutputDeviceId;
        /// <summary>
        /// Get or set the preferred video device id or empty string for
        /// default.
        /// </summary>
        public string PreferredVideoDeviceId;
        /// <summary>
        /// Get or set the preferred video format for associated video device
        /// and empty string indicates no preferred format.
        /// </summary>
        public string PreferredVideoFormatId;
        /// <summary>
        /// Get or set the preferred frame rate of the associated video device.
        /// Null indicated no preference.
        /// </summary>
        public int? PreferredFrameRate;

        /// <summary>
        /// Get or set the list of available ICE servers.
        /// </summary>
        public List<IceServer> IceServers;

        /// <summary>
        /// Get or set the local video preview element.
        /// </summary>
        public IMediaElement LocalVideoElement;
        /// <summary>
        /// Get or set the remote video element.
        /// </summary>
        public IMediaElement RemoteVideoElement;
    }
}
