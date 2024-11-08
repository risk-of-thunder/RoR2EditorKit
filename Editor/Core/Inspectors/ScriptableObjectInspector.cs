using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// Base <see cref="Inspector{T}"/> that's used for creating Inspectors for <see cref="ScriptableObject"/>s.
    /// <br>Implements a Toggle on the inspector's header that can be used to enable or disable the inspector.</br>
    /// <para>See also <see cref="IMGUIScriptableObjectInspector{T}"/> and <see cref="VisualElementScriptableObjectInspector{T}"/></para>
    /// </summary>
    /// <typeparam name="T">The type of Scriptable Object that's being inspected</typeparam>
    [InitializeOnLoad]
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