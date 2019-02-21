using ClientCore.Account;

namespace ClientCore.Factory
{
    public class SignalingFactory
    {
        private static SignalingFactory _singleton = new SignalingFactory();
        private IAccountSetupFactory _accountSetupFactory = null;

        public static SignalingFactory Singleton { get { return _singleton; } }

        public IAccountSetupFactory AccountSetupFactory
        {
            get {return _accountSetupFactory;}
            set {_accountSetupFactory = value;}
        }

        public IAccountSetup CreateIAccountSetup()
        {
            return AccountSetupFactory.Create();
        }
    }
}
