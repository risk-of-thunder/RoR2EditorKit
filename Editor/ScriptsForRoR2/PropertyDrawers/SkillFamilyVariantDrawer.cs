using RoR2;
using RoR2.Skills;
using RoR2EditorKit.PropertyDrawers;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SkillFamily.Variant))]
    public sealed class SkillFamilyVariantDrawer : IMGUIPropertyDrawer<SkillFamily.Variant>
    {
        SerializedProperty skillDefProp;
        GUIContent skillDefLabel;
        SerializedProperty unlockableDefProp;
        GUIContent unlockableDefLabel;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            skillDefProp = property.FindPropertyRelative(nameof(SkillFamily.Variant.skillDef));
            skillDefLabel = new GUIContent(skillDefProp.displayName, CreateTooltip<SkillDef>(skillDefProp.objectReferenceValue));
            unlockableDefProp = property.FindPropertyRelative(nameof(SkillFamily.Variant.unlockableDef));
            unlockableDefLabel = new GUIContent(unlockableDefProp.displayName, CreateTooltip<UnlockableDef>(unlockableDefProp.objectReferenceValue));

            EditorGUI.BeginProperty(position, label, property);

            Rect skillDefRect = new Rect(
                position.x,
                position.y,
                position.width / 2,
                position.height);
            EditorGUI.PropertyField(skillDefRect, skillDefProp, skillDefLabel);

            Rect unlockableDefRect = new Rect(
                skillDefRect.xMax,
                skillDefRect.y,
                skillDefRect.width,
                skillDefRect.height);
            EditorGUI.PropertyField(unlockableDefRect, unlockableDefProp, unlockableDefLabel);

            EditorGUI.EndProperty();
        }

        private string CreateTooltip<T>(Object obj) where T : Object
        {
            if (obj == null)
                return $"{typeof(T).Name}: Null\nType: Null";
            else
                return $"{typeof(T).Name}: {obj.name}\nType: {obj.GetType().Name}";
        }
    }
}