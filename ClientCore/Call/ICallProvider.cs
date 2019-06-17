namespace ClientCore.Call
{
    public interface ICallProvider
    {
        ICallInfo PlaceCall(CallConfiguration config);

        ICallInfo AnswerCall(CallConfiguration config, string sdpOfRemoteParty);
    }
}
