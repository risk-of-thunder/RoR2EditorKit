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
    public abstract class AddressReferencedAssetDrawer<T> : IMGUIPropertyDrawer<T> where T : AddressReferencedAsset
    {
        protected virtual string AddressTooltip { get; } = "The Address to the Asset";
        protected bool usingDirectReference;
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
        protected virtual void ModifyContextMenu(GenericMenu menu) { }

        private void OpenAddressablesDropdown(Rect rectForDropdown, SerializedProperty addressProperty)
        {

            AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), false, GetRequiredAssetType());
            dropdown.onItemSelected += (item) =>
            {
                ValidateAssetAndAssign(item, addressProperty);
            };
            dropdown.Show(rectForDropdown);
        }

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
    }
    //-----
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
    [CustomPropertyDrawer(typeof(AddressReferencedPrefab))]
    public sealed class AddressReferencedPrefabDrawer : AddressReferencedAssetDrawer<AddressReferencedPrefab>
    {
        protected override Type GetRequiredAssetType()
        {
            return typeof(GameObject);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedSpawnCard))]
    public sealed class AddressReferencedSpawnCardDrawer : AddressReferencedAssetDrawer<AddressReferencedSpawnCard>
    {
        protected override Type GetRequiredAssetType()
        {
            return typeof(SpawnCard);
        }
    }
    //-----
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