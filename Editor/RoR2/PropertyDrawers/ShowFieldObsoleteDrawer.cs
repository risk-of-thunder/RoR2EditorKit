using HG;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShowFieldObsoleteAttribute))]
    public class ShowFieldObsoleteAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            Rect extendedPosition = position;
            extendedPosition.yMax += 16;
            extendedPosition.width += 4;
            GUI.Box(extendedPosition, text: null, EditorStyles.helpBox);

            EditorGUI.HelpBox(position, "Obsolete:", MessageType.Warning);
        }
    }
}