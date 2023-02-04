using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using UnityEditor;
using RoR2EditorKit.Core.PropertyDrawers;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomPropertyDrawer(typeof(TypeRestrictedReferenceAttribute))]
    public class TypeRestrictedReferenceAttributeDrawer : IMGUIPropertyDrawer<TypeRestrictedReferenceAttribute>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new System.NotSupportedException("TypeRestrictedReferenceAttribute should only be used on fields which type is or inherits from UnityEngine.Object");
            }

            EditorGUI.BeginProperty(position, label, property);
            Object obj = EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(Object), true);
            if(SerializedPropertyFieldValue.allowedTypes.Contains(obj.GetType()))
            {
                property.objectReferenceValue = obj;
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Object Chosen", $"The field {property.displayName} can only be assigned the following objects:\n{string.Join("\n", SerializedPropertyFieldValue.allowedTypes.Select(x => x.Name))}", "Ok");
            }
            EditorGUI.EndProperty();
        }
    }
}