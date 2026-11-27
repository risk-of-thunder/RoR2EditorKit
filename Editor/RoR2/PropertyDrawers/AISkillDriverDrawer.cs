using RoR2.CharacterAI;
using UnityEditor;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AISkillDriver))]
    public sealed class AISkillDriverDrawer : NamedComponentReferencePropertyDrawer<AISkillDriver>
    {
        protected override string GetName(AISkillDriver property)
        {
            return property.customName;
        }
    }
}