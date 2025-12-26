using System;
using UnityEditor;

namespace RoR2.Editor
{
    /// <summary>
    /// A <see cref="IDisposable"/> implementation of <see cref="EditorUtility.DisplayProgressBar(string, string, float)"/>, it allows you to easily utilize a progress bar and calling the "Update" method to change the display data of the progress bar.
    /// <br></br>
    /// Due to the <see cref="IDisposable"/> implementation, this should be utilized with an using disposable statement.
    /// </summary>
    public struct DisposableProgressBar : IDisposable
    {
        private string _title;
        private string _info;
        private float _progress;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">The initial title of the Progress Bar</param>
        /// <param name="info">The initial information displayed on the Progress Bar</param>
        /// <param name="progress">The progress of the task itself, where 0 is 0% and 1 is 100%</param>
        public DisposableProgressBar(string title, string info, float progress = 0)
        {
            _title = title;
            _info = info;
            _progress = progress;

            EditorUtility.DisplayProgressBar(_title, _info, _progress);
        }

        /// <summary>
        /// Updates the ProgressBar's representation, entries are nullable and will only override the current values if not null.
        /// </summary>
        /// <param name="progress">The new value of progress, only overrides the current one if not null.</param>
        /// <param name="title">The new title for the progress bar, only overrides the current one if not null</param>
        /// <param name="info">The new information for the progress bar, only overrides the current one if not null</param>
        public void Update(float? progress, string title = null, string info = null)
        {
            _title = title ?? _title;
            _info = info ?? _info;
            _progress = progress ?? _progress;
            EditorUtility.DisplayProgressBar(_title, _info, _progress);
        }

        /// <summary>
        /// Calls <see cref="EditorUtility.ClearProgressBar"/>, effectively closing it.
        /// </summary>
        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}