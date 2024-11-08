using UnityEditor;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EntityStateMachine))]
    public class EntityStateMachineDrawer : NamedComponentReferencePropertyDrawer<EntityStateMachine>
    {
        protected override string GetName(EntityStateMachine property)
        {
            return property.customName;
        }
    }
}