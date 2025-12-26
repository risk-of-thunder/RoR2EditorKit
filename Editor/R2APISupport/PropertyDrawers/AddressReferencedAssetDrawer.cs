#if R2EK_R2API_ADDRESSABLES
using R2API.AddressReferencedAssets;
using R2API.Utils;
using RoR2.ExpansionManagement;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2.Editor.PropertyDrawers
{
    /// <summary>
    /// A Base class that's used to draw R2API's unique <see cref="AddressReferencedAsset"/>s.
    /// <para></para>
    /// The Drawer has 3 distinct modes of operation.
    /// <list type="number">
    /// <item>You can reference an asset directly, via one of your project's assets.</item>
    /// <item>You can reference an asset via it's ingame address.</item>
    /// <item>You can reference an asset via it's ingame catalog name, this is only available for certain versions of <see cref="AddressReferencedAsset"/></item>
    /// </list>
    /// You can inherit from this class to implement your own drawer for a custom <see cref="AddressReferencedAsset"/>
    /// </summary>
    /// <typeparam name="T">The type of <see cref="AddressReferencedAsset"/> that's being drawn.</typeparam>
    [CustomPropertyDrawer(typeof(AddressReferencedAsset), true)]
    public class AddressReferencedAssetDrawer : IMGUIPropertyDrawer<AddressReferencedAsset>
    {
        private static bool _useFullPathForItems;

        /// <summary>
        /// Override this string to display a custom tooltip for this AddressReferencedAsset
        /// </summary>
        protected virtual string AddressTooltip { get; } = "The Address to the Asset";

        /// <summary>
        /// Wether the asset is currently using a direct reference.
        /// </summary>
        protected bool usingDirectReference;

        /// <summary>
        /// Wether the asset can be loaded from a catalog.
        /// </summary>
        protected bool canLoadFromCatalog;

        private string _filter;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool hasNoCatalogLoad = fieldInfo.GetCustomAttribute<NoCatalogLoadAttribute>(true) != null;

            AddressableComponentRequirementAttribute componentRequirement = fieldInfo.GetCustomAttribute<AddressableComponentRequirementAttribute>(true);

            var _assetProperty = property.FindPropertyRelative("_asset");
            var _addressProperty = property.FindPropertyRelative("_address");
            var _useDirectReferenceProperty = property.FindPropertyRelative("_useDirectReference");
            var _canLoadFromCatalogProperty = property.FindPropertyRelative("_canLoadFromCatalog");

            //Fuck this shi, but if its using direct reference, nullify the address, otherwise, nullify the asset.
            if(_useDirectReferenceProperty.boolValue)
            {
                _addressProperty.stringValue = "";
            }
            else
            {
                _assetProperty.objectReferenceValue = null;
            }

            //As soon as we get the attribute, and we have the can load from catalog property, set it to false.
            if (hasNoCatalogLoad && _canLoadFromCatalogProperty != null)
            {
                _canLoadFromCatalogProperty.boolValue = false;
            }

            usingDirectReference = _useDirectReferenceProperty.boolValue;
            canLoadFromCatalog = _canLoadFromCatalogProperty?.boolValue ?? false;

            EditorGUI.BeginProperty(position, label, property);
            var fieldRect = new Rect(position.x, position.y, position.width - 16, standardPropertyHeight);

            var rectForFilterProperty = new Rect(fieldRect.x + 64, fieldRect.y + standardPropertyHeight, fieldRect.width - 64, standardPropertyHeight);


            string assetName = null;
            if (AddressablesPathDictionary.instance.TryGetPathFromGUID(_addressProperty.stringValue, out var path))
            {
                assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            }

            if (usingDirectReference)
            {
                EditorGUI.PropertyField(fieldRect, _assetProperty, new GUIContent(property.displayName));
            }
            else
            {
                _filter = EditorGUI.TextField(rectForFilterProperty, "Filter:", _filter);
                if (canLoadFromCatalog) //If the asset can load from catalog, display a regular text field in case the user wants to load the asset via name.
                {
                    string fieldDisplayName = property.displayName;
                    var ctrlRect = EditorGUI.PrefixLabel(fieldRect, new GUIContent(fieldDisplayName));
                    _addressProperty.stringValue = EditorGUI.TextField(ctrlRect, _addressProperty.stringValue);
                }
                else
                {
                    string dropdownDisplayName = assetName == null ? "None" : assetName;
                    var ctrlRect = EditorGUI.PrefixLabel(fieldRect, property.GetGUIContent());
                    if (EditorGUI.DropdownButton(ctrlRect, new GUIContent(dropdownDisplayName), FocusType.Passive))
                    {
                        OpenAddressablesDropdown(fieldRect, _addressProperty, componentRequirement);
                    }
                }
            }

            var contextRect = new Rect(fieldRect.xMax, position.y, 16, standardPropertyHeight);
            EditorGUI.DrawTextureTransparent(contextRect, R2EKConstants.AssetGUIDs.r2ekIcon, ScaleMode.ScaleToFit);
            if (Event.current.type == EventType.ContextClick)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if (contextRect.Contains(mousePos))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"Use Direct Reference"), _useDirectReferenceProperty.boolValue, () =>
                    {
                        SetBoolValue(_useDirectReferenceProperty, !_useDirectReferenceProperty.boolValue);
                    });

                    if (_canLoadFromCatalogProperty != null && !hasNoCatalogLoad)
                    {
                        menu.AddItem(new GUIContent("Can Load from Catalog"), _canLoadFromCatalogProperty.boolValue, () =>
                        {
                            SetBoolValue(_canLoadFromCatalogProperty, !_canLoadFromCatalogProperty.boolValue);
                        });
                    }

                    if (canLoadFromCatalog)
                    {
                        menu.AddItem(new GUIContent("Open Address Picker"), false, () =>
                        {
                            OpenAddressablesDropdown(fieldRect, _addressProperty, componentRequirement);
                        });
                    }
                    ModifyContextMenu(menu);
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorGUI.EndProperty();
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            base.GetPropertyHeight(property, label);
            return standardPropertyHeight * 2;
        }

        /// <summary>
        /// You can modify the context menu that appears when you click the r2ek icon here.
        /// </summary>
        /// <param name="menu">The menu that's being modified.</param>
        protected virtual void ModifyContextMenu(GenericMenu menu) { }

        private void OpenAddressablesDropdown(Rect rectForDropdown, SerializedProperty addressProperty, AddressableComponentRequirementAttribute componentRequirement)
        {
            Type requiredComponentType = componentRequirement?.requiredComponentType;
            bool searchInChildren = componentRequirement?.searchInChildren ?? false;

            AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(),
                _useFullPathForItems,
                _filter,
                requiredComponentType,
                searchInChildren,
                GetRequiredAssetTypes());

            dropdown.onItemSelected += (item) =>
            {
                ValidateAssetAndAssign(item, addressProperty);
            };
            dropdown.Show(rectForDropdown);
        }

        /// <summary>
        /// You can override this method to specify the asset types to utilize during the querying of the <see cref="AddressablesPathDictionary"/>.
        /// <br></br>
        /// By default, it uses reflection to obtain the generic parameter of the underlying <see cref="AddressReferencedAsset{T}"/>. For example, with <see cref="AddressReferencedBuffDef"/>, this will return an array with a single type, that one being <see cref="BuffDef"/>
        /// <br></br>
        /// Overriding this may be useful in case you want to accept ScriptableObjects using AddressReferencedAsset, but restrict to only specific ScriptableObjects, such as is the case with the <see cref="ItemDisplayRuleSet.KeyAssetRuleGroup.keyAssetAddress"/>
        /// </summary>
        /// <returns>An array of valid types for this field</returns>
        /// <exception cref="NullReferenceException">Thrown from the base method in case the field info does not inherit from AddressREferencedAsset<></exception>
        protected virtual Type[] GetRequiredAssetTypes()
        {
            //Get the type of the field, and a reference to AddressReferencedAsset<>
            Type fieldInfoType = propertyDrawerData.GetType();

            Type addressReferencedAssetT = typeof(AddressReferencedAsset<>);

            Type genericTypeDefinition = fieldInfoType.IsGenericType ? fieldInfoType.GetGenericTypeDefinition() : null;
            while (genericTypeDefinition != addressReferencedAssetT)
            {
                //We did not get the generic type definition match, go to the base type and try getting it.
                fieldInfoType = fieldInfoType.BaseType;
                if (fieldInfoType.BaseType == null)
                {
                    throw new NullReferenceException($"The type of {fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name} does not inherit from AddressReferencedAsset<>");
                }
                genericTypeDefinition = fieldInfoType.IsGenericType ? fieldInfoType.GetGenericTypeDefinition() : null;
            }

            //We reached the actual AddressReferencedAsset, get the type from it.
            Type[] genericArguments = fieldInfoType.GetGenericArguments();
            if (genericArguments.Length <= 0)
            {
                RoR2EKLog.Warning($"Could not automatically obtain the asset type for AddressReferencedAsset<> in {fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name}");
            }

            return genericArguments;
        }

        private void SetBoolValue(SerializedProperty property, bool booleanValue)
        {
            property.boolValue = booleanValue;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void ValidateAssetAndAssign(AddressablesPathDropdown.Item item, SerializedProperty addressProperty)
        {
            if (AddressablesPathDictionary.instance.TryGetGUIDFromPath(item.assetPath, out var guid))
            {
                addressProperty.stringValue = guid;
                addressProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AddressReferencedAssetDrawer()
        {
            _useFullPathForItems = propertyDrawerPreferenceSettings.GetOrCreateSetting("useFullNameForItems", false);
        }
    }
}
#endif