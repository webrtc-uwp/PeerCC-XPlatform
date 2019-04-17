namespace Client_UWP.Models
{
    public class MediaDeviceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public MediaDeviceModel() { }

        public MediaDeviceModel(string name)
        {
            Name = name;
        }

        public MediaDeviceModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
