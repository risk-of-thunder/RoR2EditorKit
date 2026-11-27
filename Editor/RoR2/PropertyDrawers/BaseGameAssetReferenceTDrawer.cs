using RoR2.AddressableAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using IOPath = System.IO.Path;
using UnityEngine.UIElements;
using UnityEditor.IMGUI.Controls;

#if R2EK_R2API_ADDRESSABLES
using R2API.AddressReferencedAssets;
#endif


namespace RoR2.Editor.PropertyDrawers
{
    /// <summary>
    /// The <see cref="BaseGameAssetReferenceTDrawer"/> is a custom <see cref="PropertyDrawer"/> utilized to draw a dropdown picker for <see cref="AssetReference"/>.
    /// <br></br>
    /// This property drawer will draw a Dropdown picker to pick a base game (or project, when the Addressables package is installed) asset via its address, this is only the case if the drawer is able to determine the type of the asset. In case the property drawer cannot find the type of the asset, the regular property field is drawn.
    /// <br></br>
    /// You can inherit from this class to create custom property drawers for AssetReferences.
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetReference), true)]
    public class BaseGameAssetReferenceTDrawer : ExtendedPropertyDrawer<AssetReference>
    {
        private static bool _useFullPathForItems;
        private string _filter;
        private static Regex subObjectNameExtractor = new Regex(@"(?<=\[).*?(?=\])");
#if R2EK_ADDRESSABLES
        [InitializeOnLoadMethod]
        private static void OverrideUnityAssetReferenceDrawer()
        {
            try
            {
                Type scriptAttributeUtilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
                if (scriptAttributeUtilityType == null) return;
                FieldInfo drawerTypeForTypeField = scriptAttributeUtilityType.GetField("s_DrawerTypeForType", BindingFlags.NonPublic | BindingFlags.Static);
                if (drawerTypeForTypeField == null) return;
                if (drawerTypeForTypeField.GetValue(null) == null)
                {
                    MethodInfo buildMethod = scriptAttributeUtilityType.GetMethod("BuildDrawerTypeForTypeDictionary", BindingFlags.NonPublic | BindingFlags.Static);
                    buildMethod?.Invoke(null, null);
                }
                if (drawerTypeForTypeField.GetValue(null) is not IDictionary drawerDict) return;
                Type drawerKeySetType = scriptAttributeUtilityType.GetNestedType("DrawerKeySet", BindingFlags.NonPublic | BindingFlags.Public);
                FieldInfo drawerField = drawerKeySetType?.GetField("drawer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (drawerField == null) return;
                const string unityAssetReferenceDrawerFullName = "UnityEditor.AddressableAssets.GUI.AssetReferenceDrawer";
                var keysToUpdate = new List<object>();
                foreach (DictionaryEntry entry in drawerDict)
                {
                    if (entry.Value == null) continue;
                    if (drawerField.GetValue(entry.Value) is Type drawerType && drawerType.FullName == unityAssetReferenceDrawerFullName) keysToUpdate.Add(entry.Key);
                }
                for (int i = 0; i < keysToUpdate.Count; i++)
                {
                    object key = keysToUpdate[i];
                    object boxedKeySet = drawerDict[key];
                    drawerField.SetValue(boxedKeySet, typeof(BaseGameAssetReferenceTDrawer));
                    drawerDict[key] = boxedKeySet;
                }
            }
            catch (Exception ex)
            {
                RoR2EKLog.Warning($"Failed to override Unity's AssetReferenceDrawer with BaseGameAssetReferenceTDrawer: {ex.Message}");
            }
        }
#endif
        private static void ParseSelectedItem(AddressablesPathDropdown.Item item, out string mainAssetGUID, out string subObjectName, out string subObjectType)
        {
            mainAssetGUID = "";
            subObjectName = "";
            subObjectType = "";
            if (item != null && item.isValid)
            {
                string guid = item.assetGUID;
                Match match = subObjectNameExtractor.Match(guid); //We'll match using regex to get the subObjectName
                if (match == Match.Empty)
                {
                    mainAssetGUID = guid; //There's no subObjectName, so store the mainAssetGUID;
                }
                else //We've found the subobjectname, we need to store it within its correct property.
                {
                    subObjectName = match.Value;
                    mainAssetGUID = guid.Substring(0, guid.IndexOf('['));
                    if (AddressablesPathDictionary.instance.TryGetTypeFromGUID(guid, out Type resolvedSubType)) subObjectType = resolvedSubType.AssemblyQualifiedName ?? "";
                }
            }
        }
        private static void ApplyAssetReferenceProperties(SerializedProperty property, string mainAssetGUID, string subObjectName, string subObjectType)
        {
            SerializedProperty m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
            SerializedProperty m_SubObjectNameProperty = property.FindPropertyRelative("m_SubObjectName");
            SerializedProperty m_SubObjectTypeProperty = property.FindPropertyRelative("m_SubObjectType");
            SerializedProperty m_EditorAssetChangedProperty = property.FindPropertyRelative("m_EditorAssetChanged");
            if (m_AssetGUIDProperty != null) m_AssetGUIDProperty.stringValue = mainAssetGUID;
            if (m_SubObjectNameProperty != null) m_SubObjectNameProperty.stringValue = subObjectName;
            if (m_SubObjectTypeProperty != null) m_SubObjectTypeProperty.stringValue = subObjectType;
            if (m_EditorAssetChangedProperty != null) m_EditorAssetChangedProperty.boolValue = false;
            property.serializedObject.ApplyModifiedProperties();
        }
        #region VisualElement
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type[] assetTypes = GetAssetTypes();
            if (assetTypes == null || assetTypes.Length == 0) return null;
            var result = VisualElementTemplateDictionary.instance.GetTemplateInstance("BaseGameAssetReferenceTDrawer");
            AdvancedDropdownElement element = result.Q<AdvancedDropdownElement>(); //Setup the advanced dropdown element
            element.createDropdown = (dropdownElement) =>
            {
                AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(),
                        _useFullPathForItems,
                        _filter,
                        GetRequiredComponentType(),
                        GetSearchComponentInChildren(),
                        GetAssetTypes());

                dropdown.onItemSelected += dropdownElement.OnItemSelected;
                return dropdown;
            };
            element.getDropdownButtonText = GetDropdownButtonLabel;
            element.SetLabelGUIContent(property.GetGUIContent());
            element.RegisterValueChangedCallback(evt =>
            {
                var item = evt.newValue as AddressablesPathDropdown.Item;
                ParseSelectedItem(item, out string mainAssetGUID, out string subObjectName, out string subObjectType);
                ApplyAssetReferenceProperties(property, mainAssetGUID, subObjectName, subObjectType);
            });
            element.showMixedValue = property.hasMultipleDifferentValues;
            element.value = GetCurrentValue(property);
            //Setup the filter
            TextField filter = result.Q<TextField>();
            filter.RegisterValueChangedCallback((evt) =>
            {
                _filter = evt.newValue;
            });
            return result;
        }
        #endregion

        #region IMGUI
        private struct NewPropertyValueContainer
        {
            public SerializedProperty assetGUIDProperty;
            public SerializedProperty subObjectNameProperty;

            public string assetGUID;
            public string subObjectName;
            public string subObjectType;

            public bool SerializedPropertyEqualContents(SerializedProperty _assetGUIDProperty, SerializedProperty _subObjectNameProperty)
            {
                return SerializedProperty.EqualContents(_assetGUIDProperty, assetGUIDProperty) && SerializedProperty.EqualContents(_subObjectNameProperty, subObjectNameProperty);
            }
        }

        private NewPropertyValueContainer? _newPropertyValuePair;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float standardPropertyHeight = base.GetPropertyHeight(property, label);
            Type[] typesOfAsset = GetAssetTypes();

            SerializedProperty m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
            SerializedProperty m_SubObjectNameProperty = property.FindPropertyRelative("m_SubObjectName");
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (typesOfAsset == null || typesOfAsset.Length == 0)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }

                //Draw the label for the main control
                var guiContent = property.GetGUIContent();
                var topRowRect = new Rect(position.x, position.y, position.width, standardPropertyHeight);
                var rectForDropdownControl = EditorGUI.PrefixLabel(topRowRect, guiContent);

                //Compute the rect for the filter itself, then draw it
                var rectForFilterProperty = new Rect(position.x + 32, position.y + standardPropertyHeight, position.width - 32, standardPropertyHeight);
                _filter = EditorGUI.TextField(rectForFilterProperty, "Filter:", _filter);


                //finally, draw the main control
                GUIContent dropdownButtonLabel = GetDropdownButtonLabel(GetCurrentValue(property));
                Vector2 prevIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(14, 14));
                bool dropdownClicked = EditorGUI.DropdownButton(rectForDropdownControl, dropdownButtonLabel, FocusType.Passive);
                EditorGUIUtility.SetIconSize(prevIconSize);
                if (dropdownClicked)
                {
                    AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(),
                        _useFullPathForItems,
                        _filter,
                        GetRequiredComponentType(),
                        GetSearchComponentInChildren(),
                        typesOfAsset);

                    dropdown.onItemSelected += (item) =>
                    {
                        ParseSelectedItem(item, out string mainAssetGUID, out string subObjectName, out string subObjectType);

                        _newPropertyValuePair = new NewPropertyValueContainer
                        {
                            assetGUID = mainAssetGUID,
                            subObjectName = subObjectName,
                            subObjectType = subObjectType,

                            assetGUIDProperty = m_AssetGUIDProperty,
                            subObjectNameProperty = m_SubObjectNameProperty,
                        };
                    };

                    dropdown.Show(rectForDropdownControl);
                }

                if (_newPropertyValuePair?.SerializedPropertyEqualContents(m_AssetGUIDProperty, m_SubObjectNameProperty) ?? false)
                {
                    ApplyAssetReferenceProperties(
                        property,
                        _newPropertyValuePair.Value.assetGUID,
                        _newPropertyValuePair.Value.subObjectName,
                        _newPropertyValuePair.Value.subObjectType);
                    _newPropertyValuePair = null;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float standardPropertyHeight = base.GetPropertyHeight(property, label);
            Type[] type = GetAssetTypes();

            //If the type it returns is null or has no length, we should instead display the default drawer instead, so take into account that.
            if (type == null || type.Length == 0)
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
        #endregion

        private AddressablesPathDropdown.Item GetCurrentValue(SerializedProperty property)
        {
            var m_AssetGUID = property.FindPropertyRelative("m_AssetGUID");
            var m_SubObjectNameProperty = property.FindPropertyRelative("m_SubObjectName");
            if (m_AssetGUID == null || string.IsNullOrWhiteSpace(m_AssetGUID.stringValue)) return new AddressablesPathDropdown.Item("None", "None", "", true);
            string guid;
            if (m_SubObjectNameProperty != null && !string.IsNullOrWhiteSpace(m_SubObjectNameProperty.stringValue))
            {
                guid = $"{m_AssetGUID.stringValue}[{m_SubObjectNameProperty.stringValue}]";
            }
            else
            {
                guid = m_AssetGUID.stringValue;
            }
            if (!AddressablesPathDictionary.instance.TryGetPathFromGUID(guid, out string path) || string.IsNullOrEmpty(path)) return new AddressablesPathDropdown.Item("None", "None", "", true);
            return new AddressablesPathDropdown.Item(_useFullPathForItems ? path : path.Substring(path.LastIndexOf('/') + 1), path, guid, false);
        }

        private GUIContent GetDropdownButtonLabel(AdvancedDropdownItem value)
        {
            if(value is not AddressablesPathDropdown.Item item || item.isNone)
            {
                return new GUIContent("None");
            }

            string label = "None";
            string tooltip = "";
            Texture2D icon = null;

            if(item.isValid)
            {
                tooltip = item.assetPath;
                label = tooltip.Substring(tooltip.LastIndexOf("/") + 1);
                icon = item.icon != null ? item.icon : AddressablesPathDictionary.instance.GetPreviewIconFromGUID(item.assetGUID);
            }

            return new GUIContent(label, icon, tooltip);
        }

        private Type GetRequiredComponentType()
        {
            Type type = null;
#if R2EK_R2API_ADDRESSABLES
            type = fieldInfo?.GetCustomAttribute<AddressableComponentRequirementAttribute>()?.requiredComponentType ?? null;
#endif
            return type;
        }

        private bool GetSearchComponentInChildren()
        {
            bool searchComponentInChildren = false;
#if R2EK_R2API_ADDRESSABLES
            searchComponentInChildren = fieldInfo?.GetCustomAttribute<AddressableComponentRequirementAttribute>()?.searchInChildren ?? false;
#endif
            return searchComponentInChildren;
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
            if (fieldInfo == null)
                return false;

            Type assetReferenceTType = GetElementType(fieldInfo.FieldType);
            if (assetReferenceTType == null)
                return false;

            Type cur = assetReferenceTType;
            while (cur != null && cur != typeof(object))
            {
                if (cur.IsGenericType && cur.GetGenericTypeDefinition() == typeof(AssetReferenceT<>))
                {
                    Type[] genericTypes = cur.GetGenericArguments();
                    if (genericTypes.Length > 0)
                    {
                        type = genericTypes[0];
                        return true;
                    }
                }
                cur = cur.BaseType;
            }

            var directGenericTypes = assetReferenceTType.GetGenericArguments();
            if (directGenericTypes.Length != 0)
            {
                type = directGenericTypes[0];
                return true;
            }

            return false;
        }

        private static Type GetElementType(Type fieldType)
        {
            if (fieldType == null)
                return null;
            if (fieldType.IsArray)
                return fieldType.GetElementType();
            if (typeof(IEnumerable).IsAssignableFrom(fieldType) && fieldType.IsGenericType)
                return fieldType.GenericTypeArguments.FirstOrDefault();
            return fieldType;
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