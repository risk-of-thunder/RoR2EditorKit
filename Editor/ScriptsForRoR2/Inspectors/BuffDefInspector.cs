using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RoR2EditorKit.Utilities.AssetDatabaseUtils;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(RoR2.BuffDef))]
    public sealed class BuffDefInspector : ScriptableObjectInspector<BuffDef>, IObjectNameConvention
    {
        private EliteDef eliteDef;
        private List<IMGUIContainer> eliteDefMessages = new List<IMGUIContainer>();

        private NetworkSoundEventDef networkSoundEventDef;
        private IMGUIContainer networkSoundEventdefMessage = null;

        VisualElement inspectorData = null;

        public string Prefix => "bd";

        public bool UsesTokenForPrefix => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            eliteDef = TargetType.eliteDef;
            networkSoundEventDef = TargetType.startSfx;
            serializedObject.FindProperty(nameof(BuffDef.iconPath)).stringValue = "";
            serializedObject.ApplyModifiedProperties();

            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorData = container.Q<VisualElement>("InspectorDataHolder");
            };
        }
        protected override void DrawInspectorGUI()
        {
            var eliteDef = inspectorData.Q<PropertyField>("eliteDef");
            eliteDef.RegisterCallback<ChangeEvent<EliteDef>>(CheckEliteDef);
            CheckEliteDef();

            var startSfx = inspectorData.Q<PropertyField>("startSfx");
            startSfx.RegisterCallback<ChangeEvent<NetworkSoundEventDef>>(CheckSoundEvent);
            CheckSoundEvent();
        }

        private void CheckSoundEvent(ChangeEvent<NetworkSoundEventDef> evt = null)
        {
            if(networkSoundEventdefMessage != null)
            {
                networkSoundEventdefMessage.RemoveFromHierarchy();
            }

            if (!networkSoundEventDef)
                return;

            /*if(networkSoundEventDef.eventName.IsNullOrEmptyOrWhitespace())
            {
                networkSoundEventdefMessage = CreateHelpBox($"You've associated a NetworkSoundEventDef ({networkSoundEventDef.name}) to this buff, but the EventDef's eventName is Null, Empty or Whitespace!", MessageType.Warning);
                messages.Add(networkSoundEventdefMessage);
            }*/
        }

        private void CheckEliteDef(ChangeEvent<EliteDef> evt = null)
        {
            foreach (IMGUIContainer container in eliteDefMessages)
            {
                if (container != null)
                    container.RemoveFromHierarchy();
            }
            eliteDefMessages.Clear();

            if (!eliteDef)
            {
                return;
            }

            IMGUIContainer msg = null;
            /*if(!eliteDef.eliteEquipmentDef)
            {
                msg = CreateHelpBox($"You've associated an EliteDef ({eliteDef.name}) to this buff, but the EliteDef has no EquipmentDef assigned!", MessageType.Warning);
                messages.Add(msg);
                eliteDefMessages.Add(msg);
            }

            if(eliteDef.eliteEquipmentDef && !eliteDef.eliteEquipmentDef.passiveBuffDef)
            {
                msg = CreateHelpBox($"You've associated an EliteDef ({eliteDef.name}) to this buff, but the assigned EliteDef's EquipmentDef ({eliteDef.eliteEquipmentDef.name})'s \"passiveBuffDef\" is not asigned!", MessageType.Warning);
                messages.Add(msg);
                eliteDefMessages.Add(msg);
            }

            if(eliteDef.eliteEquipmentDef && eliteDef.eliteEquipmentDef.passiveBuffDef != TargetType)
            {
                msg = CreateHelpBox($"You've associated an EliteDef ({eliteDef.name}) to this buff, but the assigned EliteDef's EquipmentDef ({eliteDef.eliteEquipmentDef.name})'s \"passiveBuffDef\" is not the inspected BuffDef!", MessageType.Warning);
                messages.Add(msg);
                eliteDefMessages.Add(msg);
            }*/
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