using System;
using Windows.Foundation;
using Windows.UI.Core;

namespace Client_UWP.Pages
{
    /// <summary>
    /// Provides ability to run the UI updates in UI thread.
    /// </summary>
    public abstract class DispatcherBindableBase : BindableBase
    {
        // The UI dispatcher
        private readonly CoreDispatcher _uiDispatcher;

        /// <summary>
        /// Creates a DispatcherBindableBase instance.
        /// </summary>
        /// <param name="uiDispatcher">Core event message dispatcher.</param>
        protected DispatcherBindableBase(CoreDispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
        }

        /// <summary>
        /// Schedules the provided callback on the UI thread from a worker thread, and 
        /// returns the results asynchronously
        /// </summary>
        /// <param name="fn">The function to execute.</param>
        protected void RunOnUiThread(Action fn)
        {
            IAsyncAction asyncOp =
                _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(fn));
        }
    }
}
