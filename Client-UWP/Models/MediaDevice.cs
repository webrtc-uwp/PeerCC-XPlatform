namespace Client_UWP.Models
{
    public class MediaDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public MediaDevice() { }

        public MediaDevice(string name)
        {
            Name = name;
        }

        public MediaDevice(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
