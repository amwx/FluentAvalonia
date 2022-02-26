using System;

namespace FluentAvalonia.UI.Controls
{
    /// <summary>
    /// Provides data for the closing event.
    /// </summary>
    public class TaskDialogClosingEventArgs : EventArgs
    {
        internal TaskDialogClosingEventArgs(TaskDialog owner, object res)
        {
            Result = res;
            _owner = owner;
        }

        /// <summary>
        /// Gets or sets a value that can cancel the closing of the dialog.
        /// A true value for Cancel cancels the default behavior.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the result of the closing event.
        /// </summary>
        public object Result { get; }

        internal bool IsDeferred => _deferral != null;

        /// <summary>
        /// Gets a <see cref="TaskDialogClosingDeferral"/> that the app can use to 
        /// respond asynchronously to the closing event.
        /// </summary>
        /// <returns></returns>
        public TaskDialogClosingDeferral GetDeferral()
        {
            _deferral = new TaskDialogClosingDeferral(_owner, Result);
            return _deferral;
        }

        private TaskDialog _owner;
        private TaskDialogClosingDeferral _deferral;
    }

    /// <summary>
    /// Represents a deferral that can be used by an app to respond asyncronously to
    /// a <see cref="TaskDialog"/> closing
    /// </summary>
    public class TaskDialogClosingDeferral
    {
        internal TaskDialogClosingDeferral(TaskDialog owner, object result)
        {
            _owner = owner;
            _result = result;
        }

        /// <summary>
        /// Notifies the system that the app has finished processing the closing event.
        /// </summary>
        public void Complete()
        {
            _owner.CompleteClosingDeferral(_result);
        }

        private TaskDialog _owner;
        private object _result;
    }
}
