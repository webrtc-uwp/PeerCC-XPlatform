using System.Collections.Generic;
using System.Drawing;
using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    class MediaVideoFormat : IMediaVideoFormat
    {
        public string Id { get; private set; }

        public Size Dimension { get; private set; }

        public IList<int> FrameRates { get; private set; }

        public string GetId(string id)
        {
            return id;
        }

        public Size GetDimension(int width, int height)
        {
            Size size = new Size();
            size.Width = width;
            size.Height = height;

            return size;
        }

        public IList<int> GetFrameRates(List<int> frameRates)
        {
            return frameRates;
        }
    }
}
