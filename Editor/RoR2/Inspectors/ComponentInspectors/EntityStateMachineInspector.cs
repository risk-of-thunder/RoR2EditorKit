using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(EntityStateMachine))]
    public class EntityStateMachineInspector : VisualElementComponentInspector<EntityStateMachine>
    {

        private SerializedProperty _customNameProperty;
        private SerializedProperty _initialStateTypeProperty;
        private SerializedProperty _mainStateTypeProperty;

        protected override void OnEnable()
        {

            base.OnEnable();
            _customNameProperty = serializedObject.FindProperty(nameof(EntityStateMachine.customName));
            _initialStateTypeProperty = serializedObject.FindProperty(nameof(EntityStateMachine.initialStateType));
            _mainStateTypeProperty = serializedObject.FindProperty(nameof(EntityStateMachine.mainStateType));
        }
        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            var customNameTextField = templateInstanceRoot.Q<TextField>();
            customNameTextField.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Populate State Types from Custom Name Standard",
                actionStatusCheck = (_) => _customNameProperty.stringValue == "Body" || _customNameProperty.stringValue == "Weapon" ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled,
                menuAction = (_) =>
                {
                    var initial = _initialStateTypeProperty.FindPropertyRelative("_typeName");
                    var main = _mainStateTypeProperty.FindPropertyRelative("_typeName");

                    switch (_customNameProperty.stringValue)
                    {
                        case "Body":
                            initial.stringValue = typeof(EntityStates.GenericCharacterSpawnState).AssemblyQualifiedName;
                            main.stringValue = targetType.TryGetComponent<CharacterMotor>(out var _) ? typeof(EntityStates.GenericCharacterMain).AssemblyQualifiedName : typeof(EntityStates.FlyState).AssemblyQualifiedName;
                            break;
                        case "Weapon":
                            initial.stringValue = typeof(EntityStates.Idle).AssemblyQualifiedName;
                            main.stringValue = typeof(EntityStates.Idle).AssemblyQualifiedName;
                            break;
                    }

                    serializedObject.ApplyModifiedProperties();
                }
            });
        }
    }
}