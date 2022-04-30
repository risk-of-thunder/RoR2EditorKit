using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EliteDef))]
    public sealed class EliteDefInspector : ScriptableObjectInspector<EliteDef>, IObjectNameConvention
    {
        VisualElement inspectorData = null;

        public string Prefix => "ed";

        public bool UsesTokenForPrefix => false;

        protected override void OnEnable()
        {
            base.OnEnable();

            OnVisualTreeCopy += () =>
            {
                var container = Find<VisualElement>("Container");
                inspectorData = Find<VisualElement>(container, "InspectorDataContainer");
            };
        }

        protected override void DrawInspectorGUI()
        {
            var equipDef = Find<PropertyField>(inspectorData, "eliteEquipmentDef");
            equipDef.RegisterCallback<ChangeEvent<EquipmentDef>>(CheckEquipDef);
            CheckEquipDef();
        }

        private void CheckEquipDef(ChangeEvent<EquipmentDef> evt = null)
        {
            /*var button = Find<Button>(color, "colorSetter");
            foreach(IMGUIContainer container in equipDefMessages)
            {
                if (container != null)
                    container.RemoveFromHierarchy();
            }
            equipDefMessages.Clear();

            IMGUIContainer msg = null;
            if(!equipmentDef)
            {
                msg = CreateHelpBox("This EliteDef has no EquipmentDef assigned! Is this intentional?", MessageType.Info);
                messages.Add(msg);
                equipDefMessages.Add(msg);
                return;
            }


            if(!equipmentDef.passiveBuffDef)
            {
                msg = CreateHelpBox($"You've assigned an EquipmentDef ({equipmentDef.name}) to this Elite, but the assigned Equipment's has no passiveBuffDef assigned!", MessageType.Warning);
                messages.Add(msg);
                equipDefMessages.Add(msg);
            }


            if(equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef != TargetType)
            {
                msg = CreateHelpBox($"You've associated an EquipmentDef ({equipmentDef.name}) to this Elite, but the assigned EquipmentDef's \"passiveBuffDef\" ({equipmentDef.passiveBuffDef.name})'s EliteDef is not the inspected EliteDef!", MessageType.Warning);
                messages.Add(msg);
                equipDefMessages.Add(msg);
            }

            button.style.display = (equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef == TargetType) ? DisplayStyle.Flex : DisplayStyle.None;*/
        }

        private void SetColor()
        {
            /*if(equipmentDef && equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef == TargetType)
            {
                TargetType.color = equipmentDef.passiveBuffDef.buffColor;
            }*/
        }

        private void SetTokens()
        {
            /*if(Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
                throw ErrorShorthands.NullTokenPrefix();

            string objName = TargetType.name.ToLowerInvariant();
            if(objName.Contains(Prefix.ToLowerInvariant()))
            {
                objName = objName.Replace(Prefix.ToLowerInvariant(), "");
            }
            TargetType.modifierToken = $"{Settings.GetPrefixUppercase()}_AFFIX_{objName.ToUpperInvariant()}";*/
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
