using System;
using UnityEditor;

namespace RoR2.Editor
{
    public class DisposableProgressBar : IDisposable
    {
        private string _title;
        private string _info;
        private float _progress;

        public DisposableProgressBar(string title, string info, float progress = 0)
        {
            _title = title;
            _info = info;
            _progress = progress;

            EditorUtility.DisplayProgressBar(_title, _info, _progress);
        }

        public void Update(float? progress, string title = null, string info = null)
        {
            _title = title ?? _title;
            _info = info ?? _info;
            _progress = progress ?? _progress;
            EditorUtility.DisplayProgressBar(_title, _info, _progress);
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}