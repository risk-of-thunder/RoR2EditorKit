using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="ComponentIMGUIEditor{T}"/> is a <see cref="BaseIMGUIEditor{T}"/> built for inspecting components.
    /// <br></br>
    /// A toggle for enabling or disabling the editor appears at the top of the UI.
    /// <para></para>
    /// Ideal for Simple Inspectors.
    /// <para></para>
    /// This editor does not support <see cref="VisualElement"/>s, if you want to make an editor for components with VisualElements, utilize <see cref="ComponentVisualElementEditor{T}"/> instead.
    /// </summary>
    /// <typeparam name="T">The type of component being edited</typeparam>
    public abstract class ComponentIMGUIEditor<T> : BaseIMGUIEditor<T> where T : Component
    {
        protected override void BeforeInspectorDrawing()
        {
            base.BeforeInspectorDrawing();
            if (!hasDoneFirstDrawing)
                return;

            Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 4);
            EditorGUI.HelpBox(controlRect, "", MessageType.None);

            Rect toggleRect = controlRect;
            toggleRect.x += 8;
            toggleRect.width -= 16;
            toggleRect.height -= 4;
            toggleRect.y += 2;

            editorEnabled = EditorGUI.ToggleLeft(toggleRect, $"Enable {ObjectNames.NicifyVariableName(target.GetType().Name)} Inspector", editorEnabled);
        }
    }
}