using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface ICallProvider
    {
        ICallInfo PlaceCall(CallConfiguration config);

        Task<ICallInfo> AnswerCallAsync(
            CallConfiguration config,
            string sdpOfRemoteParty
            );
    }
}
