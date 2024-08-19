using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(ChildLocator))]
    public class ChildLocatorInspector : IMGUIComponentInspector<ChildLocator>
    {
        SerializedProperty collectionProp;
        protected override void DrawIMGUI()
        {
            collectionProp = serializedObject.FindProperty("transformPairs");
            EditorGUILayout.PropertyField(collectionProp, false);
            if(collectionProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                collectionProp.arraySize = EditorGUILayout.DelayedIntField("Array Size", collectionProp.arraySize);
                for(int i = 0; i < collectionProp.arraySize; i++)
                {
                    var element = collectionProp.GetArrayElementAtIndex(i);
                    var name = element.FindPropertyRelative("name");
                    var transform = element.FindPropertyRelative("transform");

                    EditorGUILayout.BeginHorizontal();

                    IMGUIUtil.PropertyFieldWithSnugLabel(name, false, 20);

                    IMGUIUtil.PropertyFieldWithSnugLabel(transform, false, 20);

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}