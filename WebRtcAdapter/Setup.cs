using ClientCore.Factory;

namespace WebRtcAdapter
{
    public class Setup
    {
        public static void Install()
        {
            CallFactory.Singleton.CallProviderFactory = new Call.CallProviderFactory();

            MediaFactory.Singleton.MediaProviderFactory = new Call.MediaProviderFactory();
        }
    }
}
