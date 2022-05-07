using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    //Remove foldout of array, Set element's name to ESM's custom name
    [CustomEditor(typeof(NetworkStateMachine))]
    public sealed class NetworkStateMachineInspector : ComponentInspector<NetworkStateMachine>
    {
        ListView stateMachinesView;
        SerializedProperty stateMachines;

        VisualElement inspectorData;

        protected override void OnEnable()
        {
            base.OnEnable();
            stateMachines = serializedObject.FindProperty("stateMachines");

            OnVisualTreeCopy += () =>
            {
                inspectorData = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                SetupListView();
                /*arraySize = inspectorData.Q<IntegerField>("arraySize");
                stateMachineHolder = inspectorData.Q<VisualElement>("StateMachineHolder");*/
            };
        }
        protected override void DrawInspectorGUI()
        {
            /*var arraySize = inspectorData.Q<IntegerField>("arraySize");
            arraySize.value = stateMachines.arraySize;
            arraySize.isDelayed = true;
            arraySize.RegisterValueChangedCallback(OnArraySet);*/
        }

        /*private void OnArraySet(ChangeEvent<int> evt)
        {
            stateMachines.arraySize = evt.newValue;
            serializedObject.ApplyModifiedProperties();
            stateMachinesView.Refresh();
        }*/

        private void SetupListView()
        {
            stateMachinesView = new ListView();
            stateMachinesView.itemHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            stateMachinesView.bindingPath = "stateMachines";
            stateMachinesView.style.height = 100;
            //stateMachinesView.makeItem = CreateField;
            //stateMachinesView.bindItem = BindField;

            inspectorData.Add(stateMachinesView);

            /*VisualElement CreateField() => new ObjectField();

            void BindField(VisualElement arg1, int arg2)
            {
                try
                {
                    SerializedProperty prop = stateMachines.FindPropertyRelative($"Array.data[{arg2}]");
                    ObjectField field = arg1 as ObjectField;

                    field.SetObjectType<EntityStateMachine>();
                    field.bindingPath = prop.propertyPath;

                    if (prop != null && prop.objectReferenceValue)
                    {
                        EntityStateMachine esm = prop.objectReferenceValue as EntityStateMachine;
                        field.label = esm.customName;
                        field.tooltip = $"Initial State Type: \"{esm.initialStateType.typeName}\"" +
                            $"\n\nMain State Type: \"{esm.mainStateType.typeName}\"";
                    }
                    else
                    {
                        field.label = $"Element {arg2}";
                    }
                    field.BindProperty(serializedObject);
                }
                catch (Exception ex)
                {
                    arg1.style.display = DisplayStyle.None;
                }
            }*/
        }

        /*private void OnESMSet(ChangeEvent<UnityEngine.Object> evt)
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

        private void SetSize(ChangeEvent<int> evt = null)
        {
            stateMachines.arraySize = evt == null ? stateMachines.arraySize : evt.newValue;
            serializedObject.ApplyModifiedProperties();

            for(int i = 0; i < stateMachines.arraySize; i++)
            {
                SerializedProperty prop = stateMachines.GetArrayElementAtIndex(i);

                if (prop == null)
                    continue;

                var esm = prop.objectReferenceValue as EntityStateMachine;

                if(!esm)
                {
                    continue;
                }

                UpdateOrCreateField(esm);
            }
        }

        private void UpdateOrCreateField(EntityStateMachine esm)
        {
            if (ESMToField.TryGetValue(esm, out var field))
            {
                field.label = esm.customName;
                if(field.parent != stateMachineHolder)
                {
                    field.RemoveFromHierarchy();
                    stateMachineHolder.Add(field);
                }
            }
            else
            {
                ObjectField objField = new ObjectField();
                objField.SetObjectType<EntityStateMachine>();
                objField.label = esm.customName;
                objField.tooltip = $"Initial State Type: \"{esm.initialStateType.typeName}\"" +
                    $"\n\nMain State Type: \"{esm.mainStateType.typeName}\"";
                objField.value = esm;
                objField.RegisterValueChangedCallback(OnESMSet);
                stateMachineHolder.Add(objField);
                ESMToField.Add(esm, objField);
            }
        }*/
    }
}
