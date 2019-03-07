using ClientCore.Factory;

namespace PeerCC
{
    public class Setup
    {
        public static void Install()
        {
            SignalingFactory.Singleton.AccountProviderFactory = new Account.AccountProviderFactory();
        }
    }
}
