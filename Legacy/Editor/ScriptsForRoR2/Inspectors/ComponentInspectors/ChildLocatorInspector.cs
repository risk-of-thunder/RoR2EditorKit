using RoR2EditorKit.Inspectors;
using RoR2EditorKit.VisualElements;
using ThunderKit.Core.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ChildLocator))]
    public sealed class ChildLocatorInspector : ComponentInspector<ChildLocator>
    {
        private SerializedProperty nameTransformPairs;
        protected override void OnEnable()
        {
            base.OnEnable();
            nameTransformPairs = serializedObject.FindProperty($"transformPairs");
        }
        protected override void DrawInspectorGUI()
        {
            var listView = DrawInspectorElement.Q<ExtendedListView>("extendedListView");
            listView.CreateElement = CreateCLContainer;
            listView.BindElement = BindCLContainer;
            listView.collectionProperty = nameTransformPairs;
        }

        private void SetNamesToTransformNames(DropdownMenuAction act)
        {
            foreach (SerializedProperty property in nameTransformPairs)
            {
                var name = property.FindPropertyRelative("name");
                var transform = property.FindPropertyRelative("transform");

                if (transform.objectReferenceValue)
                {
                    name.stringValue = transform.objectReferenceValue.name;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private VisualElement CreateCLContainer() => TemplateHelpers.GetTemplateInstance("ChildLocatorEntry", null, (path) =>
        {
            return path.Contains(Constants.PackageName);
        });

        private void BindCLContainer(VisualElement arg1, SerializedProperty arg2)
        {
            var field = arg1.Q<PropertyField>("name");
            field.bindingPath = arg2.FindPropertyRelative("name").propertyPath;

            field = arg1.Q<PropertyField>("transform");
            field.bindingPath = arg2.FindPropertyRelative("transform").propertyPath;
        }
    }
}
