namespace Client_UWP.Models
{
    public class CaptureFrameRate
    {
        public string Name { get; set; }

        public CaptureFrameRate() { }

        public CaptureFrameRate(string cfr)
        {
            Name = cfr;
        }
    }
}
