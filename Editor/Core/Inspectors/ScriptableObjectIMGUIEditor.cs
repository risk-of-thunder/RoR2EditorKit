using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="ScriptableObjectIMGUIEditor{T}"/> is a <see cref="BaseIMGUIEditor{T}"/> built for inspecting ScriptableObjects.
    /// <br></br>
    /// A toggle for enabling or disabling the editor appears on the Header UI
    /// <para></para>
    /// Ideal for Simple editors.
    /// <para></para>
    /// This editor does not support <see cref="VisualElement"/>s, if you want to make an editor for ScriptableObjects with VisualElements, utilize <see cref="ScriptableObjectVisualElementEditor{T}"/> instead.
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject being edited</typeparam>
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