using RoR2.CharacterAI;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(AISkillDriver))]
    public sealed class AISkillDriverEditor : ComponentIMGUIEditor<AISkillDriver>
    {
        SerializedProperty currentFoldoutProperty = null;
        protected override void DrawIMGUI()
        {
            SerializedProperty iterator = serializedObject.GetIterator();

            bool visitChildren = true;
            while(iterator.NextVisible(visitChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    using(new EditorGUI.IndentLevelScope(currentFoldoutProperty == null ? 0 : 1))
                    {
                        if(iterator.GetFieldInfoFromProperty(out var fInfo))
                        {
                            var headerAttribute = fInfo.GetCustomAttribute<HeaderAttribute>();
                            if(headerAttribute != null)
                            {
                                currentFoldoutProperty = iterator.Copy();
                                currentFoldoutProperty.isExpanded = EditorGUILayout.Foldout(iterator.isExpanded, headerAttribute.header);
                            }
                        }

                        if(ShouldDrawProperty())
                        {
                            EditorGUILayout.PropertyField(iterator, true);
                        }
                    }
                }

                visitChildren = false;
            }
        }

        private bool ShouldDrawProperty()
        {
            if(currentFoldoutProperty == null)
            {
                return true;
            }

            return currentFoldoutProperty.isExpanded;
        }
    }
}