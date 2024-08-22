using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class ExtendedPropertyDrawer<T> : PropertyDrawer
    {
        public T propertyDrawerData
        {
            get
            {
                if(typeof(PropertyAttribute).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)attribute;
                }
                return (T)fieldInfo.GetValue(targetUnityObject);
            }
            set
            {
                if(typeof(PropertyAttribute).IsAssignableFrom(typeof(T)))
                {
                    throw new NotSupportedException("Cannot modify attribute values.");
                }
                fieldInfo.SetValue(targetUnityObject, value);
            }
        }

        public UnityEngine.Object targetUnityObject => serializedObject.targetObject;
        public SerializedObject serializedObject => serializedProperty.serializedObject;
        public SerializedProperty serializedProperty { get; private set; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            serializedProperty = property;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            serializedProperty = property;
            return null;
        }
    }
}