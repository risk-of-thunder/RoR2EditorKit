using UnityEditor;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EntityStateMachine))]
    public class EntityStateMachineDrawer : NamedObjectReferencePropertyDrawer<EntityStateMachine>
    {
        protected override string GetName(EntityStateMachine property)
        {
            return property.customName;
        }
    }
}