namespace Client_UWP.Models
{
    public class IceServer
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ServerDetails
        {
            get
            {
                if (string.IsNullOrEmpty(Port))
                    return Url;
                else
                    return Url + ":" + Port;
            }
        }

        public IceServer() { }

        public IceServer(string url, string type)
        {
            Url = url;
            Type = type;
        }

        public IceServer (string url, string type, string port)
        {
            Url = url;
            Type = type;
            Port = port;
        }
    }
}
