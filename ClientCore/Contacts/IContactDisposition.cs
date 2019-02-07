
namespace ClientCore.Contacts
{
    public enum Disposition
    {
        Added,
        Updated,
        Removed,
    }

    public interface IContactDisposition
    {
        Disposition Disposition { get; }
        IContact Contact { get; }
    }
}
