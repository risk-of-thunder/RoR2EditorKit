using KinematicCharacterController;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(KinematicCharacterMotor))]
    public class KinematicCharacterMotorInspector : ComponentIMGUIEditor<KinematicCharacterMotor>
    {
        private HideFlags _originalFlags;
        protected override void DrawIMGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                targetType.ValidateData();
            }
        }

        protected override void OnEditorEnabled()
        {
            if (!targetType || !targetType.Capsule)
                return;

            _originalFlags = targetType.Capsule.hideFlags;
            targetType.Capsule.hideFlags = HideFlags.NotEditable;
        }

        protected override void OnEditorDisabled()
        {
            if (!targetType || !targetType.Capsule)
                return;

            targetType.Capsule.hideFlags = _originalFlags;
        }
    }
}