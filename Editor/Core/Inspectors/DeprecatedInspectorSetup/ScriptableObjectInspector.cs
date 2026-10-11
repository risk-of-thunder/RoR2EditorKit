using System;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Deprecated
{
    [InitializeOnLoad]
    [Obsolete(OBSOLETE_MESSAGE)]
    public abstract class ScriptableObjectInspector<T> : Inspector<T> where T : ScriptableObject
    {
        string _nicifiedName = null;
        static ScriptableObjectInspector()
        {
            finishedDefaultHeaderGUI += DrawEnableToggle;
        }
        private static void DrawEnableToggle(UnityEditor.Editor obj)
        {
            if (obj is ScriptableObjectInspector<T> soInspector)
            {
                soInspector._nicifiedName ??= ObjectNames.NicifyVariableName(soInspector.target.GetType().Name);
                soInspector.inspectorEnabled = EditorGUILayout.ToggleLeft($"Enable {soInspector._nicifiedName} Inspector", soInspector.inspectorEnabled);
            }
        }
    }
}