using RoR2;
using RoR2EditorKit.Inspectors;
using RoR2EditorKit.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    //Remove foldout of array, Set element's name to ESM's custom name
    [CustomEditor(typeof(NetworkStateMachine))]
    public sealed class NetworkStateMachineInspector : ComponentInspector<NetworkStateMachine>
    {
        private SerializedProperty stateMachines;
        private ExtendedListView listView;

        protected override void OnEnable()
        {
            base.OnEnable();
            stateMachines = serializedObject.FindProperty($"stateMachines");
        }
        protected override void DrawInspectorGUI()
        {
            listView = DrawInspectorElement.Q<ExtendedListView>("extendedListView");
            listView.CreateElement = CreateElement;
            listView.BindElement = BindElement;
            listView.collectionProperty = stateMachines;
        }
        private VisualElement CreateElement() => new ObjectField();

        private void BindElement(VisualElement arg1, SerializedProperty arg2)
        {
            ObjectField field = arg1 as ObjectField;
            field.SetObjectType<EntityStateMachine>();
            field.bindingPath = arg2.propertyPath;

            if (arg2.objectReferenceValue)
            {
                EntityStateMachine esm = arg2.objectReferenceValue as EntityStateMachine;
                field.label = esm.customName;
                field.tooltip = $"Initial State Type: \"{esm.initialStateType.typeName}" +
                    $"\n\nMain State Type: \"{esm.mainStateType.typeName}\"";
            }
            else
            {
                field.label = $"Empty Element";
            }

            field.RegisterValueChangedCallback(OnESMSet);
        }

        private void OnESMSet(ChangeEvent<UnityEngine.Object> evt)
        {
            ObjectField field = evt.target as ObjectField;
            var obj = evt.newValue;
            if (!obj)
            {
                field.label = $"Element {field.parent.IndexOf(field)}";
                field.tooltip = "";
            }
            else
            {
                EntityStateMachine esm = evt.newValue as EntityStateMachine;
                field.label = esm.customName;
                field.tooltip = $"Initial State Type: \"{esm.initialStateType.typeName}\"" +
                    $"\n\nMain State Type: \"{esm.mainStateType.typeName}\"";
            }
        }
    }
}
