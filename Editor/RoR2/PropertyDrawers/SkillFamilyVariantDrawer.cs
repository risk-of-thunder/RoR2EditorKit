using RoR2.Skills;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SkillFamily.Variant))]
    public class SkillFamilyVariantDrawer : IMGUIPropertyDrawer<SkillFamily.Variant>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var skillDefProp = property.FindPropertyRelative("skillDef");
            var skillDefLabel = new GUIContent(skillDefProp.displayName);
            var unlockableDefProp = property.FindPropertyRelative("unlockableDef");
            var unlockableDefLabel = new GUIContent(unlockableDefProp.displayName);

            var baseRectForEach = position;
            baseRectForEach.width /= 2;

            DrawPropertyFieldWithSnugLabel(baseRectForEach, skillDefProp, skillDefLabel);

            var totalPosForUnlockableDefRect = baseRectForEach;
            totalPosForUnlockableDefRect.x = baseRectForEach.xMax;
            DrawPropertyFieldWithSnugLabel(totalPosForUnlockableDefRect, unlockableDefProp, unlockableDefLabel);
        }

        /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var skillDefProp = property.FindPropertyRelative("skillDef");
            var skillDefLabel = new GUIContent(skillDefProp.displayName);
            var unlockableDefProp = property.FindPropertyRelative("unlockableDef");
            var unlockableDefLabel = new GUIContent(unlockableDefProp.displayName);

            var totalPosForEach = position;
            totalPosForEach.width /= 2;

            var skillDefLabelPos = totalPosForEach;
            skillDefLabelPos.width = EditorStyles.label.CalcSize(skillDefLabel).x + 20;
            EditorGUI.LabelField(skillDefLabelPos, skillDefLabel);
            var skillDefFieldPos = totalPosForEach;
            skillDefFieldPos.xMin += skillDefLabelPos.width;
            EditorGUI.ObjectField(skillDefFieldPos, skillDefProp, GUIContent.none);

            var unlockableDefLabelPos = totalPosForEach;
            unlockableDefLabelPos.xMin = skillDefFieldPos.xMax;
            unlockableDefLabelPos.width = EditorStyles.label.CalcSize(unlockableDefLabel).x + 20;
            EditorGUI.LabelField(unlockableDefLabelPos, unlockableDefLabel);
            var unlockableDefFieldPos = totalPosForEach;
            unlockableDefFieldPos.x = unlockableDefLabelPos.xMax;
            unlockableDefFieldPos.width = totalPosForEach.width - unlockableDefLabelPos.width;
            EditorGUI.ObjectField(unlockableDefFieldPos, unlockableDefProp, GUIContent.none);
        }*/
    }
}