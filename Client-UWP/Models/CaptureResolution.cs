namespace Client_UWP.Models
{
    public class CaptureResolution
    {
        public string Name { get; set; }

        public CaptureResolution() { }

        public CaptureResolution(string cr)
        {
            Name = cr;
        }
    }
}
