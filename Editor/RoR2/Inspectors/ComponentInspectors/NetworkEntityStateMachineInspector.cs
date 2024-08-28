using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(NetworkStateMachine))]
    public class NetworkStateMachineInspector : IMGUIComponentInspector<NetworkStateMachine>
    {
        private SerializedProperty _stateMachines;

        protected override void OnEnable()
        {
            base.OnEnable();
            _stateMachines = serializedObject.FindProperty("stateMachines");
        }
        protected override void DrawIMGUI()
        {
            var r = EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(_stateMachines, false);
            var rectForTexture = new Rect(r.xMax - 16, r.y, 16, r.height);
            EditorGUI.DrawTextureTransparent(rectForTexture, R2EKConstants.AssetGUIDs.r2ekIcon, ScaleMode.ScaleToFit);
            if(Event.current.type == EventType.ContextClick)
            {
                if(rectForTexture.Contains(Event.current.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Auto Populate with Sibling State Machines"), true, () =>
                    {
                        var machines = targetType.GetComponents<EntityStateMachine>();
                        _stateMachines.arraySize = machines.Length;
                        for(int i = 0; i <  machines.Length; i++)
                        {
                            _stateMachines.GetArrayElementAtIndex(i).objectReferenceValue = machines[i];
                        }
                        serializedObject.ApplyModifiedProperties();
                    });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_stateMachines.isExpanded)
            {
                EditorGUILayout.BeginVertical();
                EditorGUI.indentLevel++;

                _stateMachines.arraySize = EditorGUILayout.DelayedIntField("Array Size", _stateMachines.arraySize);
                for (int i = 0; i < _stateMachines.arraySize; i++)
                {
                    DrawArrayElement(_stateMachines.GetArrayElementAtIndex(i));
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawArrayElement(SerializedProperty arrayElement)
        {
            var objectReference = arrayElement.objectReferenceValue;

            GUIContent content = objectReference ? new GUIContent(arrayElement.displayName + " | " + ((EntityStateMachine)objectReference).customName, arrayElement.tooltip) : new GUIContent(arrayElement.displayName, arrayElement.tooltip);
            EditorGUILayout.PropertyField(arrayElement, content);
        }
    }
}
