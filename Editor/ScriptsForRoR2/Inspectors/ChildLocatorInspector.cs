using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Utilities;
using ThunderKit.Core.UIElements;
using UnityEngine;
using RoR2EditorKit.Common;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ChildLocator))]
    public sealed class ChildLocatorInspector : ComponentInspector<ChildLocator>
    {
        private SerializedProperty nameTransformPairs;
        private VisualElement inspectorData;
        private ListViewHelper listView;
        protected override void OnEnable()
        {
            base.OnEnable();
            nameTransformPairs = serializedObject.FindProperty($"transformPairs");

            OnVisualTreeCopy += () =>
            {
                inspectorData = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
            };
        }
        protected override void DrawInspectorGUI()
        {
            var data = new ListViewHelper.ListViewHelperData
            {
                serializedObject = serializedObject,
                property = nameTransformPairs,
                listView = inspectorData.Q<ListView>("nameTransformPairs"),
                intField = inspectorData.Q<IntegerField>("arraySize"),
                createElement = CreateCLContainer,
                bindElement = BindCLCContainer,
            };
            listView = new ListViewHelper(data);
        }

        private VisualElement CreateCLContainer() => TemplateHelpers.GetTemplateInstance("ChildLocatorEntry", null, (path) =>
        {
            return path.Contains(Constants.PackageName);
        });
        private void BindCLCContainer(VisualElement arg1, SerializedProperty arg2)
        {
            var field = arg1.Q<PropertyField>("name");
            field.bindingPath = arg2.FindPropertyRelative("name").propertyPath;

            field = arg1.Q<PropertyField>("transform");
            field.bindingPath = arg2.FindPropertyRelative("transform").propertyPath;
        }
    }
}
