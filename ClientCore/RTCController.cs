using ClientCore.Signaling;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ClientCore
{
    /// <summary>
    /// A singleton RTCController for WebRtc session.
    /// </summary>
    public sealed class RTCController
    {
        private static RTCController instance = null;
        private static readonly object InstanceLock = new object();

        public static RTCController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new RTCController();

                    return instance;
                }
            }
        }

        private static readonly string _localPeerRandom = new Func<string>(() =>
        {
            Random random = new Random();   // WARNING: NOT cryptographically strong!
            const string chars = "abcdefghijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int length = 5;
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        })();

        static readonly Peer _localPeer = new Func<Peer>(() =>
        {
            string host = IPGlobalProperties.GetIPGlobalProperties().HostName;
            string hostname = host != null ? host : "<unknown host>";

            // A random string is added to the peer name to easily filter
            // our local peer by name when the server re-announces the
            // local peer to itself. Thus two peers with the same hostname
            // will never be the same and running the application again
            // causes a slightly different peer name on the peer list to
            // distinguish a new peer from an old zombie peer still not
            // yet purged from the server.
            string peerName = hostname + "-" + _localPeerRandom + "-data";

            return new Peer(-1, peerName);
        })();

        public Peer LocalPeer
        {
            // return a clone of the static local peer contents to ensure original Peer values cannot be modified
            get { return new Peer(_localPeer); }
        }
    }
}
