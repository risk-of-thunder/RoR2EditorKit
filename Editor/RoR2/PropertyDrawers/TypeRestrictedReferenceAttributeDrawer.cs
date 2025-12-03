using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

            using(new EditorGUI.PropertyScope(position, label, property))
            {
                Object obj = EditorGUI.ObjectField(position, property.displayName, property.objectReferenceValue, typeof(Object), true);

                //If we dont have an object, assign objectreferenceValue and continue, as we're setting it to null
                if(!obj)
                {
                    property.objectReferenceValue = obj;
                    return;
                }

                Type objectType = obj.GetType();
                if(IsTypeAllowed(objectType))
                {
                    property.objectReferenceValue = obj;
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Object Chosen", $"The field {property.displayName} can only be assigned the following objects:\n{string.Join("\n", propertyDrawerData.allowedTypes.Select(x => x.Name))}", "Ok");
                }
            }
        }

        private bool IsTypeAllowed(Type type)
        {
            Type[] allowedTypes = propertyDrawerData.allowedTypes;

            if (allowedTypes == null ||
                allowedTypes.Length == 0)
                return true;

            for(int i = 0; i < allowedTypes.Length; i++)
            {
                Type allowedType = allowedTypes[i];
                if (type.IsSubclassOf(allowedType) || type == allowedType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}