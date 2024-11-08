using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TypeRestrictedReferenceAttribute))]
    public class TypeRestrictedReferenceAttributeDrawer : IMGUIPropertyDrawer<TypeRestrictedReferenceAttribute>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new System.NotSupportedException("TypeRestrictedReferenceAttribute should only be used on fields which type is or inherits from UnityEngine.Object");
            }

            EditorGUI.BeginProperty(position, label, property);
            Object obj = EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(Object), true);
            if (((TypeRestrictedReferenceAttribute)attribute).allowedTypes.Contains(obj.GetType()))
            {
                property.objectReferenceValue = obj;
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Object Chosen", $"The field {property.displayName} can only be assigned the following objects:\n{string.Join("\n", ((TypeRestrictedReferenceAttribute)attribute).allowedTypes.Select(x => x.Name))}", "Ok");
            }
            EditorGUI.EndProperty();
        }
    }
}