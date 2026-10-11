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
    }
}