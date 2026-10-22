//If the project is using addressables we dont want to override that package's property drawer.
#if !R2EK_ADDRESSABLES
using RoR2.AddressableAssets;
using System;
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
    /// This property drawer will draw a Dropdown picker to pick a base game asset via its address, this is only the case if the drawer is able to determine the type of the asset. In case the property drawer cannot find the type of the asset, the regular property field is drawn.
    /// <br></br>
    /// You can inherit from this class to create custom property drawers for AssetReferences.
    /// <para></para>
    /// This property drawer is disabled if the Addressables package is installed.
    /// </summary>
    [CustomPropertyDrawer(typeof(AssetReference), true)]
    public class BaseGameAssetReferenceTDrawer : ExtendedPropertyDrawer<AssetReference>
    {
        private static bool _useFullPathForItems;
        private string _filter;
        private static Regex subObjectNameExtractor = new Regex(@"(?<=\[).*?(?=\])");

        #region VisualElement
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var result = VisualElementTemplateDictionary.instance.GetTemplateInstance("BaseGameAssetReferenceTDrawer");

            //Setup the advanced dropdown element
            AdvancedDropdownElement element = result.Q<AdvancedDropdownElement>();
            element.createDropdown = (element) =>
            {
                AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(),
                        _useFullPathForItems,
                        _filter,
                        GetRequiredComponentType(),
                        GetSearchComponentInChildren(),
                        GetAssetTypes());

                dropdown.onItemSelected += element.OnItemSelected;
                return dropdown;
            };
            element.getDropdownButtonText = GetDropdownButtonLabel;
            element.SetLabelGUIContent(property.GetGUIContent());
            element.RegisterValueChangedCallback(evt =>
            {
                var item = (AddressablesPathDropdown.Item)evt.newValue;

                string mainAssetGUID = "";
                string subObjectName = "";

                if (item.isValid)
                {
                    string guid = item.assetGUID;

                    //We'll match using regex to get the subObjectName
                    Match match = subObjectNameExtractor.Match(guid);
                    if (match == Match.Empty)
                    {
                        //There's no subObjectName, so store the mainAssetGUID;
                        mainAssetGUID = guid;
                    }
                    else //We've found the subobjectname, we need to store it within its correct property.
                    {
                        subObjectName = match.Value;
                        mainAssetGUID = guid.Substring(0, guid.IndexOf('['));
                    }
                }

                SerializedProperty m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
                SerializedProperty m_SubObjectNameProperty = property.FindPropertyRelative("m_SubObjectName");

                m_AssetGUIDProperty.stringValue = mainAssetGUID;
                m_SubObjectNameProperty.stringValue = subObjectName;

                property.serializedObject.ApplyModifiedProperties();
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

            public bool SerializedPropertyEqualContents(SerializedProperty _assetGUIDProperty, SerializedProperty _subObjectNameProperty)
            {
                return SerializedProperty.EqualContents(_assetGUIDProperty, assetGUIDProperty) && SerializedProperty.EqualContents(_subObjectNameProperty, subObjectNameProperty);
            }
        }

        private NewPropertyValueContainer? _newPropertyValuePair;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float standardPropertyHeight = base.GetPropertyHeight(property, label);
            serializedObject.UpdateIfRequiredOrScript();
#if R2EK_R2API_ADDRESSABLES
            AddressableComponentRequirementAttribute componentRequirement = fieldInfo.GetCustomAttribute<AddressableComponentRequirementAttribute>(true);
#endif
            Type[] typesOfAsset = GetAssetTypes();

            SerializedProperty m_AssetGUIDProperty = property.FindPropertyRelative("m_AssetGUID");
            SerializedProperty m_SubObjectNameProperty = property.FindPropertyRelative("m_SubObjectName");
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                if (typesOfAsset == null)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                    return;
                }

                //Draw the label for the main control
                var guiContent = property.GetGUIContent();
                var rectForDropdownControl = EditorGUI.PrefixLabel(position, guiContent);

                //Compute the rect for the filter itself, then draw it
                var rectForFilterProperty = new Rect(position.x + 32, position.y + standardPropertyHeight, position.width - 32, standardPropertyHeight);
                _filter = EditorGUI.TextField(rectForFilterProperty, "Filter:", _filter);


                //finally, draw the main control
                GUIContent dropdownButtonLabel = GetDropdownButtonLabel(GetCurrentValue(property));
                if (EditorGUI.DropdownButton(rectForDropdownControl, dropdownButtonLabel, FocusType.Passive))
                {
                    AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(),
                        _useFullPathForItems,
                        _filter,
                        GetRequiredComponentType(),
                        GetSearchComponentInChildren(),
                        typesOfAsset);

                    dropdown.onItemSelected += (item) =>
                    {
                        string mainAssetGUID = "";
                        string subObjectName = "";

                        if (item.isValid)
                        {
                            string guid = item.assetGUID;

                            //We'll match using regex to get the subObjectName
                            Match match = subObjectNameExtractor.Match(guid);
                            if (match == Match.Empty)
                            {
                                //There's no subObjectName, so store the mainAssetGUID;
                                mainAssetGUID = guid;
                            }
                            else //We've found the subobjectname, we need to store it within its correct property.
                            {
                                subObjectName = match.Value;
                                mainAssetGUID = guid.Substring(0, guid.IndexOf('['));
                            }
                        }

                        _newPropertyValuePair = new NewPropertyValueContainer
                        {
                            assetGUID = mainAssetGUID,
                            subObjectName = subObjectName,

                            assetGUIDProperty = m_AssetGUIDProperty,
                            subObjectNameProperty = m_SubObjectNameProperty,
                        };
                    };

                    var mousePoint = Event.current.mousePosition;
                    dropdown.Show(rectForDropdownControl);
                }

                if(_newPropertyValuePair?.SerializedPropertyEqualContents(m_AssetGUIDProperty, m_SubObjectNameProperty) ?? false)
                {
                    m_AssetGUIDProperty.stringValue = _newPropertyValuePair.Value.assetGUID;
                    m_SubObjectNameProperty.stringValue = _newPropertyValuePair.Value.subObjectName;
                    serializedObject.ApplyModifiedProperties();
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

            if(string.IsNullOrWhiteSpace(m_AssetGUID.stringValue))
            {
                return new AddressablesPathDropdown.Item("None", "None", "", true);
            }

            string guid = string.Empty;
            if(!string.IsNullOrWhiteSpace(m_SubObjectNameProperty.stringValue))
            {
                guid = $"{m_AssetGUID.stringValue}[{m_SubObjectNameProperty.stringValue}]";
            }
            else
            {
                guid = m_AssetGUID.stringValue;
            }

            string path = AddressablesPathDictionary.instance.GetPathFromGUID(guid);
            return new AddressablesPathDropdown.Item(path.Substring(path.LastIndexOf('/') + 1), path, guid, false);
        }

        private GUIContent GetDropdownButtonLabel(AdvancedDropdownItem value)
        {
            if(value is not AddressablesPathDropdown.Item item || item.isNone)
            {
                return new GUIContent("None");
            }

            string label = "None";
            string tooltip = "";

            if(item.isValid)
            {
                tooltip = item.assetPath;
                label = tooltip.Substring(tooltip.LastIndexOf("/") + 1);
            }

            return new GUIContent(label, tooltip);
        }

        private Type GetRequiredComponentType()
        {
            Type type = null;
#if R2EK_R2API_ADDRESSABLES
            type = fieldInfo.GetCustomAttribute<AddressableComponentRequirementAttribute>()?.requiredComponentType ?? null;
#endif
            return type;
        }

        private bool GetSearchComponentInChildren()
        {
            bool searchComponentInChildren = false;
#if R2EK_R2API_ADDRESSABLES
            searchComponentInChildren = fieldInfo.GetCustomAttribute<AddressableComponentRequirementAttribute>()?.searchInChildren ?? false;
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
            Type assetReferenceTType = fieldInfo.FieldType;

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