using ClientCore.Call;

namespace ClientCore.Factory
{
    public class MediaFactory
    {
        private static MediaFactory _singleton = new MediaFactory();
        private IMediaProviderFactory _mediaProviderFactory = null;

        public static MediaFactory Singleton { get { return _singleton; } }

        public IMediaProviderFactory MediaProviderFactory
        {
            get { return _mediaProviderFactory; }
            set { _mediaProviderFactory = value; }
        }

        public IMediaProvider CreateMediaProvider()
        {
            return MediaProviderFactory.Create();
        }
    }
}
