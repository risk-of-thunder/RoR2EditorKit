using RoR2;
using RoR2EditorKit.Core.Inspectors;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EquipmentDef))]
    public sealed class EquipmentDefInspector : ScriptableObjectInspector<EquipmentDef>
    {

        bool DoesNotAppear => (!TargetType.appearsInMultiPlayer && !TargetType.appearsInSinglePlayer);

        protected override bool HasVisualTreeAsset => true;

        IMGUIContainer notAppearMessage;

        VisualElement inspectorData = null;
        VisualElement tokenHolder = null;

        Button objectNameSetter;
        protected override void OnEnable()
        {
            base.OnEnable();

            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorData = container.Q<VisualElement>("InspectorDataHolder");
                tokenHolder = inspectorData.Q<Foldout>("TokenHolder");
            };
        }


        protected override void DrawInspectorGUI()
        {
            tokenHolder.AddManipulator(new ContextualMenuManipulator((x) =>
            {
                x.menu.AppendAction("Set Tokens", SetTokens, (callback) =>
                {
                    var tokenPrefix = Settings.TokenPrefix;
                    if (string.IsNullOrEmpty(tokenPrefix))
                        return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                });
            }));
        }

        private void SetTokens(DropdownMenuAction act)
        {
            if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
                throw ErrorShorthands.NullTokenPrefix();

            string tokenBase = $"{Settings.GetPrefixUppercase()}_EQUIP_{TargetType.name.ToUpperInvariant()}_";
            TargetType.nameToken = $"{tokenBase}NAME";
            TargetType.pickupToken = $"{tokenBase}PICKUP";
            TargetType.descriptionToken = $"{tokenBase}DESC";
            TargetType.loreToken = $"{tokenBase}LORE";
        }
    }
}