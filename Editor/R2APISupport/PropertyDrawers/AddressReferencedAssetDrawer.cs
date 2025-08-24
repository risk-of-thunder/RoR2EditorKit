#if R2EK_R2API_ADDRESSABLES
using R2API.AddressReferencedAssets;
using R2API.Utils;
using RoR2.ExpansionManagement;
using System;
using System.Linq;
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
    public abstract class AddressReferencedAssetDrawer<T> : IMGUIPropertyDrawer<T> where T : AddressReferencedAsset
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

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            var _assetProperty = property.FindPropertyRelative("_asset");
            var _addressProperty = property.FindPropertyRelative("_address");
            var _useDirectReferenceProperty = property.FindPropertyRelative("_useDirectReference");
            var _canLoadFromCatalogProperty = property.FindPropertyRelative("_canLoadFromCatalog");

            usingDirectReference = _useDirectReferenceProperty.boolValue;
            canLoadFromCatalog = _canLoadFromCatalogProperty?.boolValue ?? false;

            EditorGUI.BeginProperty(position, label, property);
            var fieldRect = new Rect(position.x, position.y, position.width - 16, position.height);

            string assetName = null;
            if(AddressablesPathDictionary.GetInstance().TryGetPathFromGUID(_addressProperty.stringValue, out var path))
            {
                assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            }

            if(usingDirectReference)
            {
                EditorGUI.PropertyField(fieldRect, _assetProperty, new GUIContent(property.displayName));
            }
            else
            {
                if(canLoadFromCatalog) //If the asset can load from catalog, display a regular text field in case the user wants to load the asset via name.
                {
                    string fieldDisplayName = property.displayName;
                    var ctrlRect = EditorGUI.PrefixLabel(fieldRect, new GUIContent(fieldDisplayName));
                    _addressProperty.stringValue = EditorGUI.TextField(ctrlRect, _addressProperty.stringValue);
                }
                else
                {
                    string dropdownDisplayName = assetName == null ? "None" : assetName;
                    var ctrlRect = EditorGUI.PrefixLabel(fieldRect, property.GetGUIContent());
                    if(EditorGUI.DropdownButton(ctrlRect, new GUIContent(assetName), FocusType.Passive))
                    {
                        OpenAddressablesDropdown(fieldRect, _addressProperty);
                    }
                }
            }

            var contextRect = new Rect(fieldRect.xMax, position.y, 16, position.height);
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

                    if(_canLoadFromCatalogProperty != null)
                    {
                        menu.AddItem(new GUIContent("Can Load from Catalog"), _canLoadFromCatalogProperty.boolValue, () =>
                        {
                            SetBoolValue(_canLoadFromCatalogProperty, !_canLoadFromCatalogProperty.boolValue);
                        });
                    }

                    if(canLoadFromCatalog)
                    {
                        menu.AddItem(new GUIContent("Open Address Picker"), false, () =>
                        {
                            OpenAddressablesDropdown(fieldRect, _addressProperty);
                        });
                    }
                    ModifyContextMenu(menu);
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// You can modify the context menu that appears when you click the r2ek icon here.
        /// </summary>
        /// <param name="menu">The menu that's being modified.</param>
        protected virtual void ModifyContextMenu(GenericMenu menu) { }

        private void OpenAddressablesDropdown(Rect rectForDropdown, SerializedProperty addressProperty)
        {
            AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), _useFullPathForItems, GetRequiredAssetType());
            dropdown.onItemSelected += (item) =>
            {
                ValidateAssetAndAssign(item, addressProperty);
            };
            dropdown.Show(rectForDropdown);
        }

        /// <summary>
        /// Override this method to make the <see cref="AddressablesPathDropdown"/> have a type restriction.
        /// </summary>
        /// <returns>The type of asset</returns>
        protected virtual Type GetRequiredAssetType()
        {
            return null;
        }

        private void SetBoolValue(SerializedProperty property, bool booleanValue)
        {
            property.boolValue = booleanValue;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void ValidateAssetAndAssign(AddressablesPathDropdown.Item item, SerializedProperty addressProperty)
        {
            if (AddressablesPathDictionary.GetInstance().TryGetGUIDFromPath(item.assetPath, out var guid))
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
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedBuffDef"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedBuffDef))]
    public sealed class AddressReferencedBuffDefDrawer : AddressReferencedAssetDrawer<AddressReferencedBuffDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the Buff";

        protected override Type GetRequiredAssetType()
        {
            return typeof(BuffDef);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedEliteDef"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedEliteDef))]
    public sealed class AddressReferencedEliteDefDrawer : AddressReferencedAssetDrawer<AddressReferencedEliteDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the EliteDef";

        protected override Type GetRequiredAssetType()
        {
            return typeof(EliteDef);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedEquipmentDef"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedEquipmentDef))]
    public sealed class AddressReferencedEquipmentDefDrawer : AddressReferencedAssetDrawer<AddressReferencedEquipmentDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the EquipmentDef";

        protected override Type GetRequiredAssetType()
        {
            return typeof(EquipmentDef);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedExpansionDef"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedExpansionDef))]
    public sealed class AddressReferencedExpansionDefDrawer : AddressReferencedAssetDrawer<AddressReferencedExpansionDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the ExpansionDef";

        protected override Type GetRequiredAssetType()
        {
            return typeof(ExpansionDef);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedItemDef"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedItemDef))]
    public sealed class AddressReferencedItemDefDrawer : AddressReferencedAssetDrawer<AddressReferencedItemDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the ItemDef";

        protected override Type GetRequiredAssetType()
        {
            return typeof(ItemDef);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedPrefab"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedPrefab))]
    public sealed class AddressReferencedPrefabDrawer : AddressReferencedAssetDrawer<AddressReferencedPrefab>
    {
        protected override Type GetRequiredAssetType()
        {
            return typeof(GameObject);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedSpawnCard"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedSpawnCard))]
    public sealed class AddressReferencedSpawnCardDrawer : AddressReferencedAssetDrawer<AddressReferencedSpawnCard>
    {
        protected override Type GetRequiredAssetType()
        {
            return typeof(SpawnCard);
        }
    }
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedUnlockableDef"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedUnlockableDef))]
    public sealed class AddressReferencedUnlockableDefDrawer : AddressReferencedAssetDrawer<AddressReferencedUnlockableDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the UnlockableDef";
        protected override Type GetRequiredAssetType()
        {
            return typeof(UnlockableDef);
        }
    }
#if R2EK_R2API_DIRECTOR
    //-----
    /// <summary>
    /// Custom Property Drawer for <see cref="AddressReferencedFamilyDirectorCardCategorySelection"/>
    /// </summary>
    [CustomPropertyDrawer(typeof(AddressReferencedFamilyDirectorCardCategorySelection))]
    public sealed class AddressReferencedFamilyDirectorCardCategorySelectionDrawer : AddressReferencedAssetDrawer<AddressReferencedFamilyDirectorCardCategorySelection>
    {
        protected override Type GetRequiredAssetType()
        {
            return typeof(DirectorCardCategorySelection);
        }
    }
#endif
}
#endif