using RoR2;
using RoR2EditorKit.Inspectors;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(JumpVolume))]
    public sealed class JumpVolumeInspector : ComponentInspector<JumpVolume>
    {
        private VisualElement inspectorDataContainer;
        private PropertyField targetElevationTransform;
        private PropertyField time;
        private Toggle autoCalculateJumpVelocity;
        private PropertyField jumpVelocity;

        protected override void OnEnable()
        {
            base.OnEnable();

            OnVisualTreeCopy += () =>
            {
                inspectorDataContainer = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                targetElevationTransform = inspectorDataContainer.Q<PropertyField>("targetElevationTransform");
                time = inspectorDataContainer.Q<PropertyField>("time");
                autoCalculateJumpVelocity = inspectorDataContainer.Q<Toggle>("autoCalculateJumpVelocity");
                jumpVelocity = inspectorDataContainer.Q<PropertyField>("jumpVelocity");
            };
        }
        protected override void DrawInspectorGUI()
        {
            targetElevationTransform.RegisterCallback<ChangeEvent<UnityEngine.Object>>((evt) => AutoCalculateJumpVelocity());

            time.RegisterCallback<ChangeEvent<float>>((evt) => AutoCalculateJumpVelocity());

            autoCalculateJumpVelocity.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue)
                {
                    AutoCalculateJumpVelocity();
                }
            });

            jumpVelocity.AddSimpleContextMenu(new ContextMenuData
            {
                actionStatusCheck = (dma) => TargetType.targetElevationTransform ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled,
                menuName = "Calculate Jump Velocity",
                menuAction = AutoCalculateJumpVelocity
            });
        }

        private void AutoCalculateJumpVelocity(DropdownMenuAction dma = null)
        {
            if (autoCalculateJumpVelocity.value)
            {
                if (!TargetType.targetElevationTransform)
                {
                    Debug.LogError("Cannot calculate jump velocity since there is no Transform assigned to Target Elevation Transform.");
                    return;
                }
                SerializedProperty prop = jumpVelocity.GetBindedProperty(serializedObject);
                float yInitSpeed = Trajectory.CalculateInitialYSpeed(TargetType.time, TargetType.targetElevationTransform.position.y - TargetType.gameObject.transform.position.y);
                float xOffset = TargetType.targetElevationTransform.position.x - TargetType.gameObject.transform.position.x;
                float zOffset = TargetType.targetElevationTransform.position.z - TargetType.gameObject.transform.position.z;
                prop.vector3Value = new Vector3(xOffset / TargetType.time, yInitSpeed, zOffset / TargetType.time);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
