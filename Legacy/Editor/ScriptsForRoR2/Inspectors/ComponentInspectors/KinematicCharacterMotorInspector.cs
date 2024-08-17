using KinematicCharacterController;
using RoR2EditorKit.Inspectors;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(KinematicCharacterMotor))]
    public class KinematicCharacterMotorInspector : ComponentInspector<KinematicCharacterMotor>
    {
        protected override bool HasVisualTreeAsset => false;
        private HideFlags originalFlags;
        protected override void OnEnable()
        {
            base.OnEnable();
            if (!TargetType || TargetType.Capsule)
                return;

            originalFlags = TargetType.Capsule.hideFlags;
            TargetType.Capsule.hideFlags = HideFlags.NotEditable;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!TargetType || TargetType.Capsule)
                return;

            TargetType.Capsule.hideFlags = originalFlags;
        }
        protected override void DrawInspectorGUI()
        {
            DrawInspectorElement.Add(new IMGUIContainer(() =>
            {
                EditorGUI.BeginChangeCheck();
                DrawDefaultInspector();
                if(EditorGUI.EndChangeCheck())
                {
                    TargetType.ValidateData();
                }
            }));
        }
    }
}