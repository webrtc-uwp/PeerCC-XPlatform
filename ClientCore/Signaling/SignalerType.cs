using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.Signaling
{
    public enum SignalerType
    {
        /// <summary>
        /// Supports only exchanging simple SDP offer/answer/bye messages.
        /// </summary>
        SdpMedia,
        /// <summary>
        /// Supports only exchanging simple JSON description offer/answer/bye
        /// messages.
        /// </summary>
        JsonMedia,
        /// <summary>
        /// Supports both SDP or JSON description offer/answer/bye messages.
        /// </summary>
        DualMedia,
        /// <summary>
        /// Supports an enhanced and rich content delivery mechanism.
        /// </summary>
        EnhancedContent
    }
}
