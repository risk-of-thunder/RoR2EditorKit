//If the project is using addressables we dont want to override that package's property drawer.
#if !R2EK_ADDRESSABLES
using RoR2.AddressableAssets;
using System;
using System.Text.RegularExpressions;
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
        private string filter;
        private static Regex subObjectNameExtractor = new Regex(@"(?<=\[).*?(?=\])");
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pathDictionaryInstance = AddressablesPathDictionary.GetInstance();
            Type[] typesOfAsset = GetAssetTypes();
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (typesOfAsset == null)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }

                //Draw the label for the main control
                var guiContent = property.GetGUIContent();
                var width = GetWidthForSnugLabel(guiContent);

                var rectForDropdownButtonLabel = new Rect(position.x, position.y, width, standardPropertyHeight);
                EditorGUI.PrefixLabel(rectForDropdownButtonLabel, guiContent);
                var rectForDropdownControl = new Rect(position.x + width, position.y, position.width - width, standardPropertyHeight);

                //Compute the rect for the filter itself, then draw it
                var rectForFilterProperty = new Rect(rectForDropdownControl.x, rectForDropdownControl.y + standardPropertyHeight, rectForDropdownControl.width, standardPropertyHeight);
                filter = EditorGUI.TextField(rectForFilterProperty, "Filter:", filter);

                
                //finally, draw the main control
                SerializedProperty m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
                SerializedProperty m_SubObjectNameProperty = property.FindPropertyRelative("m_SubObjectName");

                GUIContent dropdownButtonLabel = GetDropdownButtonLabel(pathDictionaryInstance, m_AssetGUIDProperty.stringValue, m_SubObjectNameProperty.stringValue);
                if (EditorGUI.DropdownButton(rectForDropdownControl, dropdownButtonLabel, FocusType.Passive))
                {
                    AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), _useFullPathForItems, filter, typesOfAsset);
                    dropdown.onItemSelected += (item) =>
                    {
                        if(!string.IsNullOrWhiteSpace(item.assetPath))
                        {
                            string guid = pathDictionaryInstance.GetGUIDFromPath(item.assetPath);
                            
                            //We'll match using regex to get the subObjectName
                            Match match = subObjectNameExtractor.Match(guid);
                            if(match == Match.Empty)
                            {
                                m_AssetGUIDProperty.stringValue = pathDictionaryInstance.GetGUIDFromPath(item.assetPath);
                            }
                            else //We've found the subobjectname, we need to store it within its correct property.
                            {
                                string subObjectName = match.Value;
                                string mainAssetGUID = guid.Substring(0, guid.IndexOf('['));

                                m_AssetGUIDProperty.stringValue = mainAssetGUID;
                                m_SubObjectNameProperty.stringValue = subObjectName;
                            }
                        }
                        else
                        {
                            m_AssetGUIDProperty.stringValue = "";
                            m_SubObjectNameProperty.stringValue = "";
                        }
                        serializedObject.ApplyModifiedProperties();
                    };

                    var mousePoint = Event.current.mousePosition;
                    dropdown.Show(rectForDropdownControl);
                }
            }
        }

        private GUIContent GetDropdownButtonLabel(AddressablesPathDictionary addressablesDictionary, string assetGUID, string subObjectName)
        {
            string label = "";
            string tooltip = "";

            if (string.IsNullOrEmpty(assetGUID))
            {
                label = "None";
                tooltip = "";
                return new GUIContent(label, tooltip);
            }

            string fullAssetPath = "";
            if(!string.IsNullOrWhiteSpace(subObjectName))
            {
                string compoundedGuid = string.Format("{0}[{1}]", assetGUID, subObjectName);
                fullAssetPath = addressablesDictionary.GetPathFromGUID(compoundedGuid);
            }
            else
            {
                fullAssetPath = addressablesDictionary.GetPathFromGUID(assetGUID);
            }

            string onlyAssetName = fullAssetPath.Substring(fullAssetPath.LastIndexOf('/') + 1);

            label = onlyAssetName;
            tooltip = fullAssetPath;

            return new GUIContent(label, tooltip);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            base.GetPropertyHeight(property, label);
            Type[] type = GetAssetTypes();

            //If the type it returns is null or has no length, we should instead display the default drawer instead, so take into account that.
            if(type == null || type.Length == 0)
            {
                var propHeight = standardPropertyHeight;
                if (property.isExpanded)
                {
                    propHeight += standardPropertyHeight * 3;
                }
                return propHeight;
            }

            //Otherwise, use the new property drawer.
            return standardPropertyHeight * 2;
        }

        [Obsolete("Use GetAssetTypes instead.")]
        protected virtual Type GetAssetType()
        {
            var result = GetAssetTypes();
            return HG.ArrayUtils.GetSafe(result, 0);
        }

        /// <summary>
        /// Override this method to specify custom asset types.
        /// <br></br>
        /// Usually just a single Type is necesary to be returned, but multiple types can be specified, this is used mainly for supporting the game's <see cref="IDRSKeyAssetReference"/>.
        /// </summary>
        /// <returns>The types of asset this asset reference requires.</returns>
        protected virtual Type[] GetAssetTypes()
        {
            if (TryGetAssetTypeFromPopertyDrawerData(out Type type))
            {
                return new Type[] { type };
            }
            return null;
        }

        /// <summary>
        /// Tries to get the AssetType directly from the AssetReferenceT we're currently drawing.
        /// </summary>
        /// <returns>True if the type was succesfully retrieved, otherwise false.</returns>
        protected bool TryGetAssetTypeFromPopertyDrawerData(out Type type)
        {
            type = null;
            Type assetReferenceTType = propertyDrawerData.GetType();

            //If the type has no generic arguments, but it inherits from AssetReferenceT, we need to get the base type and obtain the arguments. This is the case for something like AssetReferenceTexture, which inherits from AssetReferenceT<Texture>
            var genericTypes = assetReferenceTType.GetGenericArguments();
            if (genericTypes.Length == 0 && IsSubclassOfRawGeneric(typeof(AssetReferenceT<>), assetReferenceTType))
            {
                var baseType = assetReferenceTType.BaseType;
                genericTypes = baseType.GetGenericArguments();
                type = genericTypes[0];
                return true;
            }
            else if (genericTypes.Length != 0) //Otherwise, if the genericTypes is not 0, then we can get the type immediatly, this is the case for a field of type AssetReferenceT<Material>
            {
                type = genericTypes[0];
                return true;
            }
            else //Otherwise return null and draw the raw property.
            {
                return false;
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

    #region built-in subtypes
    [CustomPropertyDrawer(typeof(IDRSKeyAssetReference))]
    public class IDRSKeyAssetReferenceDrawer : BaseGameAssetReferenceTDrawer
    {
        protected override Type[] GetAssetTypes()
        {
            return new Type[]
            {
                typeof(ItemDef),
                typeof(EquipmentDef)
            };
        }
    }
    #endregion
}
#endif