using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(NetworkStateMachine))]
    public sealed class NetworkStateMachineInspector : IMGUIComponentInspector<NetworkStateMachine>
    {
        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();
            if(GUILayout.Button($"Populate from Sibling Components"))
            {
                var stateMachinesProperty = serializedObject.FindProperty("stateMachines");

                var entityStateMachines = targetType.GetComponents<EntityStateMachine>();

                stateMachinesProperty.arraySize = entityStateMachines.Length;
                for(int i = 0; i < entityStateMachines.Length; i++)
                {
                    var stateMachineProp = stateMachinesProperty.GetArrayElementAtIndex(i);
                    stateMachineProp.objectReferenceValue = entityStateMachines[i];
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}