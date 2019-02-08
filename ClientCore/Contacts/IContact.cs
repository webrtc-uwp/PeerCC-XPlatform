using ClientCore.Signaling;
using System.Collections.Generic;

namespace ClientCore.Contacts
{
    public interface IContact
    {
        /// <summary>
        /// Get a unique identifier for the contact. This identifier is
        /// uniquely assigned to the contact but is not guaranteed to remain
        /// stable across applications runs but will be stable within the
        /// same application run for the same identity.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Gets a unique identity URI representing this peer. This identity
        /// represents a stable identifier for the peer for all of time. For
        /// example, identity://domain.com/alice.
        /// </summary>
        string IdentityUri { get; }
        /// <summary>
        /// Get the peer identifier. This anonymizing identifier will be
        /// assigned to the identity URI and should be considered semi
        /// stable. This identifier should not change often but may change
        /// over longer periods of time. This value will be "" if the
        /// peer identifier is not known (where the peer cannot be
        /// contacted at this time).
        /// </summary>
        string PeerId { get; }
        /// <summary>
        /// Get the human readable display name for the user.
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Gets a friend human readable tag line for the user or returns ""
        /// when not available.
        /// </summary>
        string DisplayTagLine { get; }
        /// <summary>
        /// Get a human readable framed HTML page display summary card for this
        /// contact in a smaller partial in-context display window for the
        /// contact.
        /// </summary>
        string DisplaySummaryUri { get; }
        /// <summary>
        /// Get a URI that can be loaded in a full browser window to obtain
        /// more about this contact.
        /// </summary>
        string AboutUri { get; }
        /// <summary>
        /// Get an associated list of avatars related to this peer.
        /// </summary>
        IList<IAvatar> Avatars { get; }
        /// <summary>
        /// Get the supported communication format of the peer.
        /// </summary>
        SignalerType SupportedMessagingFormat { get; }
    }
}
