//If the project is using addressables we dont want to override that package's property drawer.
#if !R2EK_ADDRESSABLES
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using IOPath = System.IO.Path;

namespace RoR2.Editor.PropertyDrawers
{
    /// <summary>
    /// The <see cref="BaseGameAssetReferenceTDrawer"/> is a custom <see cref="PropertyDrawer"/> utilized to draw a dropdown picker for <see cref="AssetReference"/>.
    /// <br></br>
    /// This property drawer will draw a Dropdown picker to pick a base game asset via its address, this is only the case if the drawer is able to determine the type of the asset. In case the property drawer cannot find the type of the asset, the regular property field is drawn.
    /// <br></br>
    /// You can inherit from this class to create custom property drawers for AssetReferences.
    /// <para></para>
    /// This property drawer is disabled if the Addressables package is installed.
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetReference), true)]
    public class BaseGameAssetReferenceTDrawer : IMGUIPropertyDrawer<AssetReference>
    {
        private static bool _useFullPathForItems;
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pathDictionaryInstance = AddressablesPathDictionary.GetInstance();
            Type typeofAsset = GetAssetType();
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (typeofAsset == null)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }

                var widthForLabel = GetWidthForSnugLabel(property.GetGUIContent());
                EditorGUI.PrefixLabel(new Rect(position.x, position.y, widthForLabel, position.height), property.GetGUIContent());
                var rectForDropdown = new Rect(position.x + widthForLabel, position.y, position.width - widthForLabel, position.height);

                SerializedProperty m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
                string dropdownButtonLabel = string.IsNullOrWhiteSpace(m_AssetGUIDProperty.stringValue) ? "None" : IOPath.GetFileName(pathDictionaryInstance.GetPathFromGUID(m_AssetGUIDProperty.stringValue));
                if (EditorGUI.DropdownButton(rectForDropdown, new GUIContent(dropdownButtonLabel), FocusType.Passive))
                {
                    AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), _useFullPathForItems, typeofAsset);
                    dropdown.onItemSelected += (item) =>
                    {
                        if(!string.IsNullOrWhiteSpace(item.assetPath))
                        {
                            m_AssetGUIDProperty.stringValue = pathDictionaryInstance.GetGUIDFromPath(item.assetPath);
                        }
                        else
                        {
                            m_AssetGUIDProperty.stringValue = "";
                        }
                        serializedObject.ApplyModifiedProperties();
                    };

                    var mousePoint = Event.current.mousePosition;
                    dropdown.Show(rectForDropdown);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            base.GetPropertyHeight(property, label);
            Type type = GetAssetType();

            if(type != null)
            {
                return standardPropertyHeight;
            }

            var propHeight = standardPropertyHeight;
            if(property.isExpanded)
            {
                propHeight += standardPropertyHeight * 3;
            }
            return propHeight;
        }

        /// <summary>
        /// Override this method to specify a custom asset type.
        /// </summary>
        /// <returns>The type of asset this asset reference requires.</returns>
        protected virtual Type GetAssetType()
        {
            Type type = propertyDrawerData.GetType();

            //If the type has no generic arguments, but it inherits from AssetReferenceT, we need to get the base type and obtain the arguments. This is the case for something like AssetReferenceTexture, which inherits from AssetReferenceT<Texture>
            var genericTypes = type.GetGenericArguments();
            if (genericTypes.Length == 0 && IsSubclassOfRawGeneric(typeof(AssetReferenceT<>), type))
            {
                var baseType = type.BaseType;
                genericTypes = baseType.GetGenericArguments();
                return genericTypes[0];
            }
            else if(genericTypes.Length != 0) //Otherwise, if the genericTypes is not 0, then we can get the type immediatly, this is the case for a field of type AssetReferenceT<Material>
            {
                var typeOfAsset = genericTypes[0];
                return typeOfAsset;
            }
            else //Otherwise return null and draw the raw property.
            {
                return null;
            }
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public BaseGameAssetReferenceTDrawer()
        {
            _useFullPathForItems = propertyDrawerPreferenceSettings.GetOrCreateSetting("useFullNameForItems", false);
        }
    }
}
#endif