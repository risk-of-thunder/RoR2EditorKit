using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(ItemDef))]
    public class ItemDefInspector : VisualElementScriptableObjectInspector<ItemDef>
    {


        private SerializedProperty deprecatedTierEnumProperty;
        private SerializedProperty nameTokenProperty;
        private SerializedProperty pickupTokenProperty;
        private SerializedProperty descTokenProperty;
        private SerializedProperty loreTokenProperty;
        private PropertyField itemTierDefField;
        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            deprecatedTierEnumProperty = serializedObject.FindProperty("deprecatedTier");
            nameTokenProperty = serializedObject.FindProperty(nameof(ItemDef.nameToken));
            pickupTokenProperty = serializedObject.FindProperty(nameof(ItemDef.pickupToken));
            descTokenProperty = serializedObject.FindProperty(nameof(ItemDef.descriptionToken));
            loreTokenProperty = serializedObject.FindProperty(nameof(ItemDef.loreToken));

            templateInstanceRoot.Q<PropertyField>("DeprecatedTier").RegisterCallback<ChangeEvent<string>>(OnPropertyFieldChanged);
            itemTierDefField = templateInstanceRoot.Q<PropertyField>("ItemTierDef");
            OnPropertyFieldChanged(null);

            var foldout = templateInstanceRoot.Q<Foldout>("TokenContainer");

            foldout.AddSimpleContextMenu(new ContextMenuData
            {
                actionStatusCheck = (_) => R2EKSettings.instance.tokenExists ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled,
                menuAction = AssignTokens,
                menuName = "Generate Tokens",
            });
        }

        private void AssignTokens(DropdownMenuAction action)
        {
            var prefixToken = R2EKSettings.instance.GetTokenAllUpperCase();

            var objName = serializedObject.targetObject.name;
            var properName = objName.ToUpperInvariant().Replace(" ", "");
            var stringToFormat = prefixToken + "_ITEM_" + properName + "_{0}";

            nameTokenProperty.stringValue = string.Format(stringToFormat, "NAME");
            pickupTokenProperty.stringValue = string.Format(stringToFormat, "PICKUP");
            descTokenProperty.stringValue = string.Format(stringToFormat, "DESC");
            loreTokenProperty.stringValue = string.Format(stringToFormat, "LORE");

            serializedObject.ApplyModifiedProperties();
        }

        private void OnPropertyFieldChanged(ChangeEvent<string> evt)
        {
            var newValue = (ItemTier)GetPropertyValue(deprecatedTierEnumProperty);
            itemTierDefField.SetDisplay(newValue == ItemTier.AssignedAtRuntime);
        }

        private object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                default:
                    return null;
            }
        }
    }
}