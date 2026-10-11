using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    [InitializeOnLoad]
    public abstract class ScriptableObjectIMGUIEditor<T> : BaseIMGUIEditor<T> where T : ScriptableObject
    {
        string _nicifiedName = null;
        static ScriptableObjectIMGUIEditor()
        {
            finishedDefaultHeaderGUI += DrawEnableToggle;
        }

        private static void DrawEnableToggle(UnityEditor.Editor editor)
        {
            if (editor is ScriptableObjectIMGUIEditor<T> soInspector)
            {
                soInspector._nicifiedName ??= ObjectNames.NicifyVariableName(soInspector.target.GetType().Name);
                soInspector.editorEnabled = EditorGUILayout.ToggleLeft($"Enable {soInspector._nicifiedName} Inspector", soInspector.editorEnabled);
            }
        }
    }
}