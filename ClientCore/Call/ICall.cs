using System.Drawing;
using System.Threading.Tasks;

namespace ClientCore.Call
{
    public delegate void ResolutionChangeHandler(
        MediaDirection direction,
        Size dimension
        );
    public delegate void FrameRateChangeHandler(
        MediaDirection direction,
        int frameRate
        );

    public interface ICall
    {
        Task HangupAsync();

        event FrameRateChangeHandler OnFrameRateChanged;
        event ResolutionChangeHandler OnResolutionChanged;
    }
}
