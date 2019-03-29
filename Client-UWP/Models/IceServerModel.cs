namespace Client_UWP.Models
{
    public class IceServerModel
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Username { get; set; }
        public string Credential { get; set; }

        public IceServerModel() {}

        public IceServerModel(string url, string username, string credential)
        {
            Url = url;
            Username = username;
            Credential = credential;
        }
    }
}
