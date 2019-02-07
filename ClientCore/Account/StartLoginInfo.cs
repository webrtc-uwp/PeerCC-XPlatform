
namespace ClientCore.Account
{
    public class StartLoginInfo
    {
        /// <summary>
        /// Gets or sets the login URI specific to the login service.
        /// </summary>
        public string ServiceUri { get; set; }

        /// <summary>
        /// Gets or sets if the browser control window starts visible. If true
        /// then the browser control window starts as a visible dialog on the
        /// end user's screen. Otherwise false indicates the browser control
        /// window is hidden by default and can only be made visible by
        /// calling the built in JavaScript function to show the hidden
        /// browser window.
        /// </summary>
        public bool BrowserControlVisible { get; set; }
        /// <summary>
        /// Gets or sets the client JavaScript browser method that the login
        /// service may call from a spawned browser control to cause the
        /// browser window to change from a hidden view to a HTML dialog
        /// window.
        /// </summary>
        public string JSBuiltInShowHiddenBrowserWindowFunctionName { get; set; }
        /// <summary>
        /// Gets or sets the client JavaScript browser method that the login
        /// service may call from a spawned browser control to notify the
        /// client application the browser login has completed.
        /// This built in function must accept a string parameter containing
        /// the login complete payload information that will be used to obtain
        /// information about the logged in identity.
        /// </summary>
        public string JSBuiltInLoginCompleteFunctionName { get; set; }

        /// <summary>
        /// Gets or sets the previous cached credential information about this
        /// identity (if available). The payload is only interpretable by the
        /// login service.
        /// </summary>
        public string CachedCredentialsPayload { get; set; }
    }
}
