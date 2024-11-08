using EntityStates;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableEntityStateType))]
    internal sealed class SerializableEntityStateTypePropertyDrawer : IMGUIPropertyDrawer<SerializableEntityStateType>
    {
        private static bool _useFullName = false;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsFullyQualified(property))
            {
                FullyQualify(property);
            }

            var _drawer = new SerializableEntityStateTypeDrawer();
            _drawer.useFullName = _useFullName;

            _drawer.onTypeSelectedHandler += (item) =>
            {
                property.FindPropertyRelative("_typeName").stringValue = item.assemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            };
            _drawer.DoEditorGUI(position, property.FindPropertyRelative("_typeName").stringValue, label);
        }

        private bool IsFullyQualified(SerializedProperty property)
        {
            var p = property.FindPropertyRelative("_typeName");
            var typeName = p.stringValue;
            return Type.GetType(typeName) != null;
        }

        private void FullyQualify(SerializedProperty property)
        {
            var p = property.FindPropertyRelative("_typeName");
            var typeName = p.stringValue;
            var t = Type.GetType($"{typeName}, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            if (t != null) //Type found, fully qualify it.
            {
                p.stringValue = t.AssemblyQualifiedName;
                p.serializedObject.ApplyModifiedProperties();
            }
        }

        public SerializableEntityStateTypePropertyDrawer()
        {
            _useFullName = propertyDrawerPreferenceSettings.GetOrCreateSetting("useFullNameForItems", false);
        }
    }

    public class SerializableEntityStateTypeDrawer
    {
        public bool useFullName;
        public event Action<InheritingTypeSelectDropdown.Item> onTypeSelectedHandler;

        public void DoEditorGUI(Rect position, string assemblyQualifiedName, GUIContent label)
        {
            var typeName = Type.GetType(assemblyQualifiedName)?.Name ?? "No State Set";

            var prefixRect = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(prefixRect, new GUIContent(typeName, assemblyQualifiedName), FocusType.Passive))
            {
                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), typeof(EntityState), useFullName);
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

                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), typeof(EntityState), useFullName);
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
    }
}