using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="ScriptableObjectVisualElementEditor{T}"/> is a <see cref="BaseVisualElementEditor{T}"/> built for inspector ScriptableObjects
    /// <br></br>
    /// A toggle for enabling or disabling the editor appears on the Header UI
    /// <para></para>
    /// Ideal for Complex editors
    /// <para></para>
    /// Utilizing <see cref="IMGUIContainer"/> is possible, but not recommended, if you want to make an editor for ScriptableObjects with IMGUI, utilize <see cref="ScriptableObjectIMGUIEditor{T}"/> instead.
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject being edited</typeparam>
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