using ClientCore.Call;

namespace ClientCore.Factory
{
    public class CallFactory
    {
        private static CallFactory _singleton = new CallFactory();
        private ICallProviderFactory _callProviderFactory = null;

        public static CallFactory Singleton { get { return _singleton; } }

        public ICallProviderFactory CallProviderFactory
        {
            get { return _callProviderFactory; }
            set { _callProviderFactory = value; }
        }

        public ICallProvider CreateICallProvider()
        {
            return CallProviderFactory.Create();
        }
    }
}
