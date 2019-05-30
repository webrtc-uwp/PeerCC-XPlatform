using System.Collections.Generic;
using System.Drawing;
using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class MediaVideoFormat : IMediaVideoFormat
    {
        public string Id { get; private set; }

        public Size Dimension { get; private set; }

        public IList<int> FrameRates { get; private set; }

        public void GetId(string id) => Id = id;

        public void GetDimension(int width, int height)
        {
            Size size = new Size();
            size.Width = width;
            size.Height = height;

            Dimension = size;
        }

        public void GetFrameRates(List<int> frameRates) => FrameRates = frameRates;
    }
}
