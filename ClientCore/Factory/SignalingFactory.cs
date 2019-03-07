using ClientCore.Account;

namespace ClientCore.Factory
{
    public class SignalingFactory
    {
        private static SignalingFactory _singleton = new SignalingFactory();
        private IAccountProviderFactory _accountProviderFactory = null;

        public static SignalingFactory Singleton { get { return _singleton; } }

        public IAccountProviderFactory AccountSetupFactory
        {
            get { return _accountProviderFactory; }
            set { _accountProviderFactory = value; }
        }

        public IAccountProvider CreateIAccountProvider()
        {
            return AccountSetupFactory.Create();
        }
    }
}
