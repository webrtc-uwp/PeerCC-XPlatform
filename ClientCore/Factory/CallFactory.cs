using ClientCore.Account;

namespace ClientCore.Factory
{
    public class CallFactory
    {
        private static CallFactory _singleton = new CallFactory();
#if CHANGE
        private IAccountSetupFactory _accountSetupFactory = null;
#endif

        public static CallFactory Singleton { get { return _singleton; } }

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
