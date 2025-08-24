using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// An Editor only PropertyAttribute that marks that a string field should have a button, and that button can be used to specify a path.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FilePickerPath : PropertyAttribute
    {
        /// <summary>
        /// Optional, the title of the window displayed when the button is clicked
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Optional, the default name for the Folder/File in the window when the button is clicked
        /// </summary>
        public string defaultName { get; set; }
        /// <summary>
        /// Optional, the extension type for the File in the window when the button is clicked
        /// </summary>
        public string extension
        {
            get
            {
                if(_extension.StartsWith('.'))
                {
                    return _extension.TrimStart('.');
                }
                return _extension;
            }
            set
            {
                _extension = value;
            }
        }
        private string _extension;

        /// <summary>
        /// What method from EditorUtility should be used
        /// </summary>
        public PickerType pickerType { get; set; }

        /// <summary>
        /// Represents the different options EditorUtility has for displaying the window
        /// </summary>
        public enum PickerType
        {
            /// <summary>
            /// Calls <see cref="EditorUtility.OpenFolderPanel(string, string, string)"/> with <see cref="title"/>, <see cref="Application.dataPath"/> and <see cref="defaultName"/> as the arguments.
            /// </summary>
            OpenFolder,

            /// <summary>
            /// Calls <see cref="EditorUtility.SaveFolderPanel(string, string, string)"/> with <see cref="title"/>, <see cref="Application.dataPath"/> and <see cref="defaultName"/> as the arguments.
            /// </summary>
            SaveFolder,

            /// <summary>
            /// Calls <see cref="EditorUtility.OpenFilePanel(string, string, string)"/> with <see cref="title"/>, <see cref="Application.dataPath"/> and <see cref="extension"/> as the arguments
            /// </summary>
            OpenFile,

            /// <summary>
            /// Calls <see cref="EditorUtility.SaveFilePanel(string, string, string, string)"/> with <see cref="title"/>, <see cref="Application.dataPath"/>, <see cref="defaultName"/> and <see cref="extension"/> as the arguments
            /// </summary>
            SaveFile
        }

        /// <summary>
        /// Constructor for a FilePickerPath attribute
        /// </summary>
        /// <param name="_pickerType">The type of EditorUtility method to use for the drawer</param>
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
                if (GUI.Button(buttonRect, "...", EditorStyles.miniButton))
                {
                    string path = "";
                    switch (attribute.pickerType)
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


                    if (!string.IsNullOrEmpty(path))
                    {
                        property.stringValue = path;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
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
                    switch (attribute.pickerType)
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

                    if (!string.IsNullOrEmpty(path))
                    {
                        property.stringValue = path;
                        property.serializedObject.ApplyModifiedProperties();
                    }
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