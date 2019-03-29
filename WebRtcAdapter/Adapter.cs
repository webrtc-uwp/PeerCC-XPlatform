using ClientCore.Call;
using Org.WebRtc;
using System.Collections.Generic;

namespace WebRtcAdapter
{
    public class Adapter
    {
        private static Adapter instance = null;
        private static readonly object InstanceLock = new object();

        public static Adapter Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new Adapter();

                    return instance;
                }
            }
        }

        private Adapter()
        {
            _iceServers = new List<RTCIceServer>();
        }

        public readonly List<RTCIceServer> _iceServers;

        public void ConfigureIceServers(List<IceServer> iceServers)
        {
            _iceServers.Clear();

            foreach (IceServer iceServer in iceServers)
            {
                RTCIceServer server = new RTCIceServer();
                server.Urls = iceServer.Urls;
                server.Username = iceServer.Username;
                server.Credential = iceServer.Credential;
                _iceServers.Add(server);
            }
        }
    }
}
