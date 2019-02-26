using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace PeerCC.Signaling
{
    /// <summary>
    /// The connection state.
    /// </summary>
    public enum State
    {
        NotConnected,
        Resolving,
        SigningIn,
        Connected,
        SigningOutWaiting,
        SigningOut
    };

    public static class Extensions
    {
        public static int ParseLeadingInt(this string str)
        {
            return int.Parse(Regex.Match(str, "\\d+").Value);
        }

        public static int Remove<T>(this ObservableCollection<T> obsCollection, Func<T, bool> condition)
        {
            var itemsToRemove = obsCollection.Where(condition).ToList();
            foreach (var item in itemsToRemove)
            {
                obsCollection.Remove(item);
            }
            return itemsToRemove.Count;
        }
    }
}
