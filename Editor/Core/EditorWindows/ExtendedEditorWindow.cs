using System;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        public EditorSetting windowProjectSettings
        {
            get
            {
                if (_windowProjectSettings == null)
                {
                    _windowProjectSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.ProjectSetting);
                }
                return _windowProjectSettings;
            }
        }
        private EditorSetting _windowProjectSettings;

        public EditorSetting windowPreferenceSettings
        {
            get
            {
                if (_windowPreferenceSettings == null)
                {
                    _windowPreferenceSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.UserSetting);
                }
                return _windowPreferenceSettings;
            }
        }
        private EditorSetting _windowPreferenceSettings;

        protected SerializedObject serializedObject
        {
            get
            {
                return _serializedObject;
            }
            set
            {
                if(_serializedObject != value)
                {
                    _serializedObject = value;
                    OnSerializedObjectChanged();
                    return;
                }
            }
        }
        private SerializedObject _serializedObject;

        public static TEditorWindow Open<TEditorWindow>(UnityEngine.Object serializedObjectOverride = null) where TEditorWindow : ExtendedEditorWindow
        {
            TEditorWindow window = GetWindow<TEditorWindow>();
            window.serializedObject = new SerializedObject(serializedObjectOverride ? serializedObjectOverride : window);
            window.OnWindowOpened();
            return window;
        }

        protected virtual void OnSerializedObjectChanged() { }

        protected abstract void CreateGUI();

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void Awake() { }
        protected virtual void OnWindowOpened() { }

        public struct OpeningContext
        {
            public UnityEngine.Object desiredSerializedObject;
            public bool isUtility;
            public string titleOverride;
            public Type[] desiredDocking;
        }
    }
}
