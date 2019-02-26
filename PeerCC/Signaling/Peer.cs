using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerCC.Signaling
{
    public class Peer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Peer() { }

        public Peer(Peer peer)
        {
            Id = peer.Id;
            Name = peer.Name;
        }

        public Peer(int id, string name)
        {
            Id = id;
            Name = name.Replace(":", ";");
        }

        public Peer(int id, string name, string message)
        {
            Id = id;
            Name = name.Replace(":", ";");
        }

        public override string ToString()
        {
            return Id + ":" + Name;
        }

        public static Peer CreateFromString(string peerAsString)
        {
            string[] separatingChars = { ":" };
            string[] words = peerAsString.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);

            return new Peer(System.Convert.ToInt32(words[0]), words[1]);
        }
    }
}
