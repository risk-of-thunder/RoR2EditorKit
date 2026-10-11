using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    [InitializeOnLoad]
    public abstract class ScriptableObjectVisualElementEditor<T> : BaseVisualElementEditor<T> where T : ScriptableObject
    {
        string _nicifiedName = null;
        static ScriptableObjectVisualElementEditor()
        {
            finishedDefaultHeaderGUI += DrawEnableToggle;
        }

        private static void DrawEnableToggle(UnityEditor.Editor editor)
        {
            if(editor is ScriptableObjectVisualElementEditor<T> soInspector)
            {
                soInspector._nicifiedName ??= ObjectNames.NicifyVariableName(soInspector.target.GetType().Name);
                soInspector.editorEnabled = EditorGUILayout.ToggleLeft($"Enable {soInspector._nicifiedName} Inspector", soInspector.editorEnabled);
            }
        }
    }
}