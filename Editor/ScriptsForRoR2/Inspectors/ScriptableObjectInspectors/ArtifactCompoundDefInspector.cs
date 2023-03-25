using RoR2;
using RoR2EditorKit.Inspectors;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ArtifactCompoundDef))]
    public sealed class ArtifactCompoundDefInspector : ScriptableObjectInspector<ArtifactCompoundDef>, IObjectNameConvention
    {
        public string Prefix => "acd";
        public bool UsesTokenForPrefix => false;

        private VisualElement inspectorDataHolder;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorDataHolder = container.Q<VisualElement>("InspectorDataContainer");
            };
        }

        protected override void DrawInspectorGUI()
        {
            var compoundValue = inspectorDataHolder.Q<ValidatingPropertyField>("value");
            SetupValidator(compoundValue);
            compoundValue.ForceValidation();

            compoundValue.AddSimpleContextMenu(new ContextMenuData(
                "Use RNG for Value",
                dma =>
                {
                    var valueProp = serializedObject.FindProperty("value");
                    valueProp.intValue = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                    serializedObject.ApplyModifiedProperties();
                }));
        }

        private void SetupValidator(ValidatingPropertyField compoundValue)
        {
            compoundValue.AddValidator(() => GetValue() == 1, MessageMaker(1, "Circle"), MessageType.Error);

            compoundValue.AddValidator(() => GetValue() == 3, MessageMaker(3, "Triangle"), MessageType.Error);

            compoundValue.AddValidator(() => GetValue() == 5, MessageMaker(5, "Diamond"), MessageType.Error);

            compoundValue.AddValidator(() => GetValue() == 7, MessageMaker(7, "Square"), MessageType.Error);

            compoundValue.AddValidator(() => GetValue() == 11, MessageMaker(11, "Empty"), MessageType.Error);


            int GetValue() => compoundValue.ChangeEvent == null ? TargetType.value : (int)compoundValue.ChangeEvent.newValue;
            string MessageMaker(int value, string vanillaType) => $"Compound value cannot be {value}, as that value is reserved for the {vanillaType} compound";
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