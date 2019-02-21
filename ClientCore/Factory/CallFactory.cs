using ClientCore.Account;

namespace ClientCore.Factory
{
    public class CalllFactory
    {
        private static CalllFactory _singleton = new CalllFactory();
#if CHANGE
        private IAccountSetupFactory _accountSetupFactory = null;
#endif

        public static CalllFactory Singleton { get { return _singleton; } }

#if CHANGE
        public IAccountSetupFactory AccountSetupFactory
        {
            get {return _accountSetupFactory;}
            set {_accountSetupFactory = value;}
        }

        public IAccountSetup CreateIAccountSetup()
        {
            return AccountSetupFactory.Create();
        }
#endif
    }
}
