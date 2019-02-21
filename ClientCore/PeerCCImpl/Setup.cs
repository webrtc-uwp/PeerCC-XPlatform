using ClientCore.Factory;

namespace ClientCore.PeerCCImpl
{
    public class Setup
    {
        public static void Install()
        {
            SignalingFactory.Singleton.AccountSetupFactory = new ClientCore.PeerCCImpl.Account.AccountSetupFactory();
        }
    }
}
