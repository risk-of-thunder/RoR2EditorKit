using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using RoR2EditorKit.Core.Inspectors;
using RoR2;
using System;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ArtifactCompoundDef))]
    public sealed class ArtifactCompoundDefInspector : ScriptableObjectInspector<ArtifactCompoundDef>, IObjectNameConvention
    {
        public string Prefix => "acd";
        public bool UsesTokenForPrefix => false;

        private VisualElement inspectorDataHolder;

        private IMGUIContainer valueMessageContainer;

        private Button objectNameSetter;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorDataHolder = container.Q<VisualElement>("InspectorDataHolder");
            };
        }

        protected override void DrawInspectorGUI()
        {
            var value = inspectorDataHolder.Q<PropertyField>("value");
            value.RegisterCallback<ChangeEvent<int>>(OnValueSet);
            OnValueSet();
            value.AddManipulator(new ContextualMenuManipulator(BuildValueMenu));
        }

        private void BuildValueMenu(ContextualMenuPopulateEvent obj)
        {
            obj.menu.AppendAction("Use RNG for Value", (dma) =>
            {
                var valueProp = serializedObject.FindProperty("value");
                valueProp.intValue = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                serializedObject.ApplyModifiedProperties();
            });
        }

        private void OnValueSet(ChangeEvent<int> evt = null)
        {
            int value = evt == null ? TargetType.value : evt.newValue;

            if(valueMessageContainer != null)
            {
                valueMessageContainer.RemoveFromHierarchy();
            }

            switch(value)
            {
                case 1:
                    valueMessageContainer = CompoundHelpBox(value, "Circle");
                    break;
                case 5:
                    valueMessageContainer = CompoundHelpBox(value, "Diamond");
                    break;
                case 11:
                    valueMessageContainer = CompoundHelpBox(value, "Empty");
                    break;
                case 7:
                    valueMessageContainer = CompoundHelpBox(value, "Square");
                    break;
                case 3:
                    valueMessageContainer = CompoundHelpBox(value, "Triangle");
                    break;
                default:
                    valueMessageContainer = null;
                    break;
            }

            if(valueMessageContainer != null)
            {
                DrawInspectorElement.Add(valueMessageContainer);
            }

            IMGUIContainer CompoundHelpBox(int v, string name) => CreateHelpBox($"Compound value cannot be {v}, as that value is reserved for the {name} compound", MessageType.Error);
        }

        public PrefixData GetPrefixData()
        {
            return new PrefixData(() =>
            {
                var origName = TargetType.name;
                TargetType.name = Prefix + origName;
                AssetDatabaseUtils.UpdateNameOfObject(TargetType);
            });
        }
    }
}