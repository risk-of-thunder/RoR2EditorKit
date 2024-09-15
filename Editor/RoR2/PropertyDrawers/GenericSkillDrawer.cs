using UnityEditor;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(GenericSkill))]
    public class GenericSkillDrawer : NamedObjectReferencePropertyDrawer<GenericSkill>
    {
        protected override string GetName(GenericSkill property)
        {
            return property.skillName;
        }
    }
}