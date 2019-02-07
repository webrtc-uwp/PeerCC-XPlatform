using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface ICallInfo
    {
        /// <summary>
        /// Get the associated call object.
        /// </summary>
        ICall Call { get; }

        /// <summary>
        /// Get the offer or answer SDP.
        /// </summary>
        string Sdp { get; }
    }
}
