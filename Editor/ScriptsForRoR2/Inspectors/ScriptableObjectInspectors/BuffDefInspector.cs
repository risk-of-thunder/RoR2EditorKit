using RoR2;
using RoR2EditorKit.Inspectors;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.VisualElements;
using UnityEngine.UIElements;
using static RoR2EditorKit.AssetDatabaseUtils;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(RoR2.BuffDef))]
    public sealed class BuffDefInspector : ScriptableObjectInspector<BuffDef>, IObjectNameConvention
    {
        private ValidatingPropertyField eliteDefValidator;
        private ValidatingPropertyField startSfxValidator;
        private VisualElement inspectorData = null;

        public string Prefix => "bd";

        public bool UsesTokenForPrefix => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            serializedObject.FindProperty(nameof(BuffDef.iconPath)).stringValue = "";
            serializedObject.ApplyModifiedProperties();

            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorData = container.Q<VisualElement>("InspectorDataContainer");
                eliteDefValidator = inspectorData.Q<ValidatingPropertyField>("eliteDef");
                startSfxValidator = inspectorData.Q<ValidatingPropertyField>("startSfx");
            };
        }
        protected override void DrawInspectorGUI()
        {
            var color = inspectorData.Q<PropertyField>("buffColor");
            color.AddSimpleContextMenu(new ContextMenuData(
                "Set Color to Elite Color",
                SetColor,
                statusCheck =>
                {
                    if (TargetType.eliteDef)
                        return DropdownMenuAction.Status.Normal;
                    return DropdownMenuAction.Status.Hidden;
                }));

            SetupEliteValidator(eliteDefValidator);
            eliteDefValidator.ForceValidation();

            SetupSfxValidator(startSfxValidator);
            startSfxValidator.ForceValidation();
        }

        private void SetColor(DropdownMenuAction act)
        {
            TargetType.buffColor = TargetType.eliteDef.color;
        }

        private void SetupEliteValidator(ValidatingPropertyField validator)
        {
            validator.AddValidator(() =>
            {
                var ed = GetEliteDef();
                return ed && !ed.eliteEquipmentDef;
            },
            $"You've associated an EliteDef to this buff, but the EliteDef has no EquipmentDef assigned!", MessageType.Warning);

            validator.AddValidator(() =>
            {
                var ed = GetEliteDef();
                return ed && ed.eliteEquipmentDef && !ed.eliteEquipmentDef.passiveBuffDef;
            },
            "You've assigned an EliteDef to this buff, but the EliteDef's EquippmentDef has no assigned passiveBuffDef!", MessageType.Warning);

            validator.AddValidator(() =>
            {
                var ed = GetEliteDef();
                return ed && ed.eliteEquipmentDef && ed.eliteEquipmentDef.passiveBuffDef && ed.eliteEquipmentDef.passiveBuffDef != TargetType;
            }, $"You've associated an EliteDef to this buff, but the assigned EliteDef's EquippmentDef's ppassiveBuffDef is not the inspected buffDef!", MessageType.Warning);

            EliteDef GetEliteDef()
            {
                if(validator.ChangeEvent == null)
                {
                    return TargetType.eliteDef;
                }
                else
                {
                    return (EliteDef)validator.ChangeEvent.newValue;
                }
            }
        }

        private void SetupSfxValidator(ValidatingPropertyField validator)
        {
            validator.AddValidator(() =>
            {
                var nsed = GetEventDef();
                return nsed && nsed.eventName.IsNullOrEmptyOrWhitespace();
            },
            $"You've associated a NetworkSoundEventDef to this buff, but the EventDef's eventName is Null, Empty or Whitespace!", MessageType.Warning);

            NetworkSoundEventDef GetEventDef() => validator.ChangeEvent == null ? TargetType.startSfx : (NetworkSoundEventDef)validator.ChangeEvent.newValue;
        }

        public PrefixData GetPrefixData()
        {
            return new PrefixData(() =>
            {
                var origName = TargetType.name;
                TargetType.name = Prefix + origName;
                UpdateNameOfObject(TargetType);
            });
        }
    }
}