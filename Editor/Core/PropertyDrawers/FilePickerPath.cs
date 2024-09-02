using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FilePickerPath : PropertyAttribute
    {
        public string title { get; set; }
        public string defaultName { get; set; }
        public string extension { get; set; }
        public PickerType pickerType { get; set; }
        public enum PickerType
        {
            OpenFolder,
            SaveFolder,
            OpenFile,
            SaveFile
        }

        public FilePickerPath(PickerType _pickerType)
        {
            pickerType = _pickerType;
        }
        [CustomPropertyDrawer(typeof(FilePickerPath))]
        private class Drawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var attribute = this.attribute as FilePickerPath;

                var posForProp = position;
                posForProp.width -= 20;
                EditorGUI.PropertyField(posForProp, property);

                var buttonRect = new Rect(posForProp.xMax, position.y, 20, position.y);
                if(GUI.Button(buttonRect, "...", EditorStyles.miniButton))
                {
                    string path = "";
                    switch(attribute.pickerType)
                    {
                        case PickerType.OpenFile:
                            path = EditorUtility.OpenFilePanel(attribute.title, Application.dataPath, attribute.extension);
                            break;
                        case PickerType.SaveFile:
                            path = EditorUtility.SaveFilePanel(attribute.title, Application.dataPath, attribute.defaultName, attribute.extension);
                            break;
                        case PickerType.OpenFolder:
                            path = EditorUtility.OpenFolderPanel(attribute.title, Application.dataPath, attribute.defaultName);
                            break;
                        case PickerType.SaveFolder:
                            path = EditorUtility.SaveFolderPanel(attribute.title, Application.dataPath, attribute.defaultName);
                            break;
                    }

                    if(!string.IsNullOrEmpty(path))
                    {
                        property.stringValue = FileUtil.GetProjectRelativePath(path);
                    }
                }
                base.OnGUI(position, property, label);
            }
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                var attribute = this.attribute as FilePickerPath;
                var container = new VisualElement()
                {
                    name = property.propertyPath + "Container",
                };
                container.style.flexDirection = FlexDirection.Row;

                var textField = new TextField(property.displayName);
                textField.BindProperty(property);
                textField.style.flexGrow = 1;
                container.Add(textField);

                var button = new Button(() =>
                {
                    string path = "";
                    switch(attribute.pickerType)
                    {
                        case PickerType.OpenFile:
                            path = EditorUtility.OpenFilePanel(attribute.title, Application.dataPath, attribute.extension);
                            break;
                        case PickerType.SaveFile:
                            path = EditorUtility.SaveFilePanel(attribute.title, Application.dataPath, attribute.defaultName, attribute.extension);
                            break;
                        case PickerType.OpenFolder:
                            path = EditorUtility.OpenFolderPanel(attribute.title, Application.dataPath, attribute.defaultName);
                            break;
                        case PickerType.SaveFolder:
                            path = EditorUtility.SaveFolderPanel(attribute.title, Application.dataPath, attribute.defaultName);
                            break;
                    }

                    if(!string.IsNullOrEmpty(path))
                        textField.value = FileUtil.GetProjectRelativePath(path);
                });

                button.name = property.propertyPath + "FilePathButton";
                button.style.flexGrow = 0;
                button.text = "...";
                container.Add(button);

                return container;
            }
        }
    }
}