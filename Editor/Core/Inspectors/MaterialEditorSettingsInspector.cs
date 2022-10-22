using RoR2EditorKit.Settings;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.Inspectors
{
    //This is also fucking stupid
    [CustomEditor(typeof(MaterialEditorSettings))]
    internal sealed class MaterialEditorSettingsInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return StaticInspectorGUI(serializedObject);
        }

        internal static VisualElement StaticInspectorGUI(SerializedObject serializedObject, bool forSettingsWindow = false)
        {
            VisualElement mainContainer = new VisualElement();
            VisualElement shaderContainer = new VisualElement();

            var enabledProp = serializedObject.FindProperty(nameof(MaterialEditorSettings.EnableMaterialEditor));
            var propertyField = new PropertyField(enabledProp);
            propertyField.RegisterCallback<ChangeEvent<bool>>((evt) => shaderContainer.SetDisplay(evt.newValue));
            if (forSettingsWindow)
            {
                propertyField.AddToClassList("thunderkit-field-input");
            }
            mainContainer.Add(propertyField);

            SerializedProperty settings = serializedObject.FindProperty(nameof(MaterialEditorSettings.shaderStringPairs));
            foreach (SerializedProperty prop in settings)
            {
                var propField = new PropertyField(prop);

                if (forSettingsWindow)
                    propField.AddToClassList("thunderkit-field-input");

                shaderContainer.Add(propField);
            }

            mainContainer.Add(shaderContainer);
            if (forSettingsWindow)
            {
                mainContainer.AddToClassList("thunderkit-field");
                mainContainer.style.flexDirection = FlexDirection.Column;
            }

            return mainContainer;
        }
    }
}
