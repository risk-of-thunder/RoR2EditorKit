using EntityStates;
using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableSystemType))]
    internal sealed class SerializableSystemTypePropertyDrawer : IMGUIPropertyDrawer<SerializableSystemType>
    {
        private bool _triedToFullyQualify = false;
        private SerializableSystemTypeDrawer _drawer;
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_triedToFullyQualify)
            {
                _triedToFullyQualify = true;
                FullyQualify(property);
                return;
            }

            _drawer = _drawer ?? new SerializableSystemTypeDrawer(GetBaseType());

            _drawer.onTypeSelectedHandler += (item) =>
            {
                property.FindPropertyRelative("assemblyQualifiedName").stringValue = item.assemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            };
            _drawer.DoEditorGUI(position, property.FindPropertyRelative("assemblyQualifiedName").stringValue, label);
        }

        private Type GetBaseType()
        {
            var attribute = fieldInfo.GetCustomAttribute<SerializableSystemType.RequiredBaseTypeAttribute>();
            if(attribute != null)
            {
                return attribute.type;
            }
            return null;
        }

        private void FullyQualify(SerializedProperty property)
        {
            var assemblyQualifiedNameProp = property.FindPropertyRelative("assemblyQualifiedName");
            var typeName = assemblyQualifiedNameProp.stringValue;
            Type t = Type.GetType(typeName);

            if (t != null) //Fully qualified, we good.
                return;
            else //Try obtaining the type by specifying the ror2 assembly.
                t = Type.GetType($"{typeName}, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            if (t != null) //Type found, fully qualify it.
            {
                assemblyQualifiedNameProp.stringValue = t.AssemblyQualifiedName;
                assemblyQualifiedNameProp.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public class SerializableSystemTypeDrawer
    {
        public Type requiredBaseType { get; private set; }
        public event Action<InheritingTypeSelectDropdown.Item> onTypeSelectedHandler;

        public void DoEditorGUI(Rect position, string assemblyQualifiedName, GUIContent label)
        {
            var typeName = Type.GetType(assemblyQualifiedName)?.Name ?? "No Type Set";

            var prefixRect = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(prefixRect, new GUIContent(typeName, assemblyQualifiedName), FocusType.Passive))
            {
                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), requiredBaseType);
                dropdown.onItemSelected += (item) =>
                {
                    onTypeSelectedHandler?.Invoke(item);
                };
                dropdown.Show(position);
            }
        }

        public void DoIMGUILayout(string assemblyQualifiedName, GUIContent label)
        {
            var typeName = Type.GetType(assemblyQualifiedName)?.Name ?? "No State Type";

            var rect = EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            if (EditorGUILayout.DropdownButton(new GUIContent(typeName, assemblyQualifiedName), FocusType.Passive))
            {
                Rect labelRect = GUILayoutUtility.GetLastRect();

                var rectToUse = new Rect
                {
                    x = rect.x + labelRect.width,
                    y = rect.y,
                    height = rect.height,
                    width = rect.width - labelRect.width
                };

                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), requiredBaseType);
                dropdown.onItemSelected += (item) => onTypeSelectedHandler?.Invoke(item);
                dropdown.Show(rectToUse);
            }
            EditorGUILayout.EndHorizontal();
        }

        public VisualElement DoVisualElement(string assemblyQualifiedName, GUIContent label)
        {
            return new IMGUIContainer(() =>
            {
                DoIMGUILayout(assemblyQualifiedName, label);
            });
        }

        public SerializableSystemTypeDrawer(Type requiredBaseType)
        {
            this.requiredBaseType = requiredBaseType;
        }
    }
}