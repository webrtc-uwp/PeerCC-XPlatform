using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GuiCore.Utilities
{
    /// <summary>
    /// Utility class to organize SDP negotiation.
    /// </summary>
    class SdpUtils
    {
        /// <summary>
        /// Forces the SDP to use the selected audio and video codecs.
        /// </summary>
        /// <param name="sdp">Session description.</param>
        /// <param name="audioCodecId">Audio codec.</param>
        /// <param name="videoCodecId">Video codec.</param>
        /// <returns>True id succeeds to force to use the selected audio/video codecs.</returns>
        public static bool SelectCodecs(ref string sdp, int audioCodecId, int videoCodecId)
        {
            Regex mfdRegex = new Regex("\r\nm=audio.*RTP.*?( .\\d*)+\r\n");
            Match mfdMatch = mfdRegex.Match(sdp);
            List<string> mfdListToErase = new List<string>(); //mdf = media format descriptor
            bool audioMediaDescFound = mfdMatch.Groups.Count > 1; //Group 0 is whole match

            if (audioMediaDescFound)
            {
                if (audioCodecId < 0)
                {
                    return false;
                }
                for (int groupCtr = 1/*Group 0 is whole match*/; groupCtr < mfdMatch.Groups.Count; groupCtr++)
                {
                    for (int captureCtr = 0; captureCtr < mfdMatch.Groups[groupCtr].Captures.Count; captureCtr++)
                    {
                        mfdListToErase.Add(mfdMatch.Groups[groupCtr].Captures[captureCtr].Value.TrimStart());
                    }
                }
                if (!mfdListToErase.Remove(audioCodecId.ToString()))
                {
                    return false;
                }
            }

            mfdRegex = new Regex("\r\nm=video.*RTP.*?( .\\d*)+\r\n");
            mfdMatch = mfdRegex.Match(sdp);
            bool videoMediaDescFound = mfdMatch.Groups.Count > 1; //Group 0 is whole match
            if (videoMediaDescFound)
            {
                if (videoCodecId < 0)
                {
                    return false;
                }
                for (int groupCtr = 1/*Group 0 is whole match*/; groupCtr < mfdMatch.Groups.Count; groupCtr++)
                {
                    for (int captureCtr = 0; captureCtr < mfdMatch.Groups[groupCtr].Captures.Count; captureCtr++)
                    {
                        mfdListToErase.Add(mfdMatch.Groups[groupCtr].Captures[captureCtr].Value.TrimStart());
                    }
                }
                if (!mfdListToErase.Remove(videoCodecId.ToString()))
                {
                    return false;
                }
            }

            if (audioMediaDescFound)
            {
                // Alter audio entry
                Regex audioRegex = new Regex("\r\n(m=audio.*RTP.*?)( .\\d*)+");
                sdp = audioRegex.Replace(sdp, "\r\n$1 " + audioCodecId);
            }

            if (videoMediaDescFound)
            {
                // Alter video entry
                Regex videoRegex = new Regex("\r\n(m=video.*RTP.*?)( .\\d*)+");
                sdp = videoRegex.Replace(sdp, "\r\n$1 " + videoCodecId);
            }

            // Remove associated rtp mapping, format parameters, feedback parameters
            Regex removeOtherMdfs = new Regex("a=(rtpmap|fmtp|rtcp-fb):(" + String.Join("|", mfdListToErase) + ") .*\r\n");
            sdp = removeOtherMdfs.Replace(sdp, "");

            return true;
        }
    }
}
