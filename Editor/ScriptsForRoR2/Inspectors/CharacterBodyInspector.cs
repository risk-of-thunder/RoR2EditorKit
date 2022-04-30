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

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(CharacterBody))]
    public sealed class CharacterBodyInspector : ComponentInspector<CharacterBody>
    {
        private VisualElement inspectorData;

        protected override void DrawInspectorGUI()
        {
            inspectorData = DrawInspectorElement.Q<VisualElement>("inspectorData");

            inspectorData.Q<Button>("tokenSetter").clicked += SetTokens;

            var rootMotionInState = inspectorData.Q<Toggle>("rootMotion");
            rootMotionInState.RegisterValueChangedCallback(OnRootMotionSet);
            OnRootMotionSet();

        }

        private void SetTokens()
        {
            if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                throw ErrorShorthands.NullTokenPrefix();
            }

            GameObject go = TargetType.gameObject;
            if(go)
            {
                TargetType.baseNameToken = $"{Settings.TokenPrefix.ToUpperInvariant()}_{go.name.ToUpperInvariant()}_NAME";
                TargetType.subtitleNameToken = $"{Settings.TokenPrefix.ToUpperInvariant()}_{go.name.ToUpperInvariant()}_SUBTITLE";
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        private void OnRootMotionSet(ChangeEvent<bool> evt = null)
        {
            var rootSpeed = inspectorData.Q<FloatField>("mainRootSpeed");
            bool value = evt == null ? inspectorData.Q<Toggle>("rootMotion").value : evt.newValue;
            rootSpeed.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
