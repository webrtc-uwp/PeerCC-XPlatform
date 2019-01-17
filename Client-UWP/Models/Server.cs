namespace Client_UWP.Models
{
    public class Server
    {
        private string _ip;
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
    }
}
