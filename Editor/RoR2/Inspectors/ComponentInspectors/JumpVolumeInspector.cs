using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(JumpVolume))]
    public class JumpVolumeInspector : ComponentIMGUIEditor<JumpVolume>
    {
        private const string SETTING_NAME = "autoCalculateJumpVelocity";
        private bool autoCalculateJumpVelocity => editorProjectSettings.GetOrCreateSetting(SETTING_NAME, true);
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
            DrawJumpVolumeSettings();
            EditorGUILayout.Space(4);
            DrawJumpVelocitySettings();
        }

        private void DrawJumpVolumeSettings()
        {
            EditorGUILayout.LabelField("Jump Volume Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.acceptTeam)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.jumpSoundString)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.AllowAirControl)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.disableFallDamage)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.onJump)));
        }

        private void DrawJumpVelocitySettings()
        {
            EditorGUILayout.LabelField($"Jump Velocity Settings", EditorStyles.boldLabel);

            if (IMGUIUtil.CreateFieldForSetting(editorProjectSettings, SETTING_NAME, true))
            {
                AutoCalculateJumpVelocity();
            }

            {
                using var changeScope = new EditorGUI.ChangeCheckScope();
                EditorGUILayout.PropertyField(_targetElevationTransformProperty);
                EditorGUILayout.PropertyField(_timeProperty);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.alignJumpVelocityToTransform)));

                if (changeScope.changed)
                    AutoCalculateJumpVelocity();
            }

            using(new EditorGUI.DisabledGroupScope(autoCalculateJumpVelocity))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(JumpVolume.jumpVelocity)));
            }
        }

        private void AutoCalculateJumpVelocity()
        {
            if (!autoCalculateJumpVelocity)
                return;

            Transform t = _targetElevationTransformProperty.objectReferenceValue as Transform;
            if (!t)
            {
                RoR2EKLog.Error($"Cannot calculate jump velocity since there is no Transform assigned to Target Elevation Transform.");
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