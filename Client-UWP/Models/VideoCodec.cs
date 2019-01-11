namespace Client_UWP.Models
{
    public class VideoCodec
    {
        public string Name { get; set; }

        public VideoCodec() { }

        public VideoCodec(string vc)
        {
            Name = vc;
        }
    }
}
