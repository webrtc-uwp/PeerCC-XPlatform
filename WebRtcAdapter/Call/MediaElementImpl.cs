using ClientCore.Call;
using Windows.UI.Xaml.Controls;

namespace WebRtcAdapter.Call
{
    public class MediaElementImpl : IMediaElement
    {
        private MediaElement _mediaElement { get; set; }

        public static IMediaElement GetMediaElement(MediaElement video)
        {
            var mediaElementImpl = new MediaElementImpl();
            mediaElementImpl._mediaElement = video;

            return mediaElementImpl;
        }
    }
}
