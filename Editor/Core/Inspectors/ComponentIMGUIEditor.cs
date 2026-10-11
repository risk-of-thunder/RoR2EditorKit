using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    public abstract class ComponentIMGUIEditor<T> : BaseIMGUIEditor<T> where T : Component
    {
        protected override void OnBeforeInspectorDrawing()
        {
            base.OnBeforeInspectorDrawing();
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