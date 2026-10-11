using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(ChildLocator))]
    public class ChildLocatorInspector : IMGUIComponentInspector<ChildLocator>
    {
        SerializedProperty collectionProp;
        protected override void DrawIMGUI()
        {
            collectionProp = serializedObject.FindProperty("transformPairs");

            DrawDefaultInspector();

            if(GUILayout.Button($"Fill empty names with transform names"))
            {
                for(int i = 0; i < collectionProp.arraySize; i++)
                {
                    SerializedProperty nameTransformPairProp = collectionProp.GetArrayElementAtIndex(i);

                    SerializedProperty nameProp = nameTransformPairProp.FindPropertyRelative(nameof(ChildLocator.NameTransformPair.name));
                    SerializedProperty transformProp = nameTransformPairProp.FindPropertyRelative(nameof(ChildLocator.NameTransformPair.transform));

                    if(string.IsNullOrWhiteSpace(nameProp.stringValue) && transformProp.objectReferenceValue)
                    {
                        nameProp.stringValue = transformProp.objectReferenceValue.name;
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}