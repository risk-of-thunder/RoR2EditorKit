using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(JumpVolume))]
    public class JumpVolumeInspector : IMGUIComponentInspector<JumpVolume>
    {
        private const string SETTING_NAME = "autoCalculateJumpVelocity";
        private bool autoCalculateJumpVelocity => inspectorProjectSettings.GetOrCreateSetting(SETTING_NAME, true);
        private SerializedProperty _targetElevationTransformProperty;
        private SerializedProperty _timeProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _targetElevationTransformProperty = serializedObject.FindProperty(nameof(JumpVolume.targetElevationTransform));
            _timeProperty = serializedObject.FindProperty(nameof(JumpVolume.time));
        }
        protected override void DrawIMGUI()
        {
            if (IMGUIUtil.CreateFieldForSetting(inspectorProjectSettings, SETTING_NAME, true))
            {
                AutoCalculateJumpVelocity();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_targetElevationTransformProperty);
            EditorGUILayout.PropertyField(_timeProperty);
            if (EditorGUI.EndChangeCheck())
            {
                AutoCalculateJumpVelocity();
            }

            EditorGUI.BeginDisabledGroup(autoCalculateJumpVelocity);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.jumpVelocity)));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.jumpSoundString)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.onJump)));
        }

        private void AutoCalculateJumpVelocity()
        {
            if (!autoCalculateJumpVelocity)
                return;

            Transform t = _targetElevationTransformProperty.objectReferenceValue as Transform;
            if (!t)
            {
                Debug.LogError($"Cannot calculate jump velocity since there is no Transform assigned to Target Elevation Transform.");
                return;
            }
            Transform myT = targetType.transform;

            float yInitSpeed = Trajectory.CalculateInitialYSpeed(_timeProperty.floatValue, t.position.y - myT.position.y);
            float xOffset = t.position.x - myT.position.x;
            float zOffset = t.position.z - myT.position.z;
            serializedObject.FindProperty(nameof(JumpVolume.jumpVelocity)).vector3Value = new Vector3
            {
                x = xOffset / _timeProperty.floatValue,
                y = yInitSpeed,
                z = zOffset / _timeProperty.floatValue
            };
            serializedObject.ApplyModifiedProperties();
        }
    }
}