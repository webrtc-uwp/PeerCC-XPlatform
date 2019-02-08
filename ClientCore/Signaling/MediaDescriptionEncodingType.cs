using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
