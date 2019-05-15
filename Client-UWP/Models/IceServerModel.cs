using System.Collections.Generic;

namespace Client_UWP.Models
{
    public class IceServerModel
    {
        public List<string> Urls { get; set; }
        public string Username { get; set; }
        public string Credential { get; set; }

        public IceServerModel() {}

        public IceServerModel(List<string> urls, string username, string credential)
        {
            Urls = urls;
            Username = username;
            Credential = credential;
        }
    }
}
