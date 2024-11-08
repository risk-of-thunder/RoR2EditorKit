using RoR2.Skills;
using UnityEditor;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(SkillFamily))]
    public class SkillFamilyInspector : IMGUIScriptableObjectInspector<SkillFamily>
    {
        SerializedProperty defaultVariantIndex;
        SerializedProperty variantsArray;
        int chosenIndex;

        protected virtual void OnEnable()
        {
            defaultVariantIndex = serializedObject.FindProperty("defaultVariantIndex");
            variantsArray = serializedObject.FindProperty(nameof(SkillFamily.variants));
        }
        protected override void DrawIMGUI()
        {
            EditorGUILayout.PropertyField(variantsArray);

            chosenIndex = EditorGUILayout.Popup(defaultVariantIndex.displayName, defaultVariantIndex.intValue, GetDisplayedOptions());
            defaultVariantIndex.intValue = chosenIndex;
        }

        protected string[] GetDisplayedOptions()
        {
            string[] result = new string[variantsArray.arraySize];
            for (int i = 0; i < result.Length; i++)
            {
                var variantProp = variantsArray.GetArrayElementAtIndex(i);
                var variantSkillDef = variantProp.FindPropertyRelative("skillDef");
                var skilldefValue = (SkillDef)variantSkillDef.objectReferenceValue;
                result[i] = skilldefValue ? string.IsNullOrEmpty(skilldefValue.skillName) ? variantSkillDef.objectReferenceValue.name : skilldefValue.skillName : $"INDEX {i} NULL";
            }
            return result;
        }
    }
}