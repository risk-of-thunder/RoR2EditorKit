#if R2EK_R2API_ADDRESSABLES
using R2API.AddressReferencedAssets;
using R2API.Utils;
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

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            var _assetProperty = property.FindPropertyRelative("_asset");
            var _addressProperty = property.FindPropertyRelative("_address");
            var _useDirectReferenceProperty = property.FindPropertyRelative("_useDirectReference");
            var _canLoadFromCatalogProperty = property.FindPropertyRelative("_canLoadFromCatalog");

            usingDirectReference = _useDirectReferenceProperty.boolValue;

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
                string displayName = assetName == null ? property.displayName : property.displayName + $" ({assetName})";
                EditorGUI.PropertyField(fieldRect, _addressProperty, new GUIContent(displayName, AddressTooltip));
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

                    menu.AddItem(new GUIContent("Open Address Picker"), false, () =>
                    {
                        AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), false, GetRequiredAssetType());
                        dropdown.onItemSelected += (item) =>
                        {
                            ValidateAssetAndAssign(item, _addressProperty, property);
                        };
                        dropdown.Show(fieldRect);
                    });
                    ModifyContextMenu(menu);
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
            EditorGUI.EndProperty();
        }
        protected virtual void ModifyContextMenu(GenericMenu menu) { }

        protected virtual Type GetRequiredAssetType()
        {
            return null;
        }

        protected virtual bool IsAssetValid(string guidOrAddressablePath)
        {
            return true;
        }

        protected bool DoesAssetInheritFrom<T1>(string guidOrAddressablePath) where T1 : UnityEngine.Object
        {
            var resourceLocation = Addressables.LoadResourceLocationsAsync(guidOrAddressablePath).WaitForCompletion().FirstOrDefault();
            return resourceLocation.ResourceType.IsSameOrSubclassOf<T1>();
        }

        private void SetBoolValue(SerializedProperty property, bool booleanValue)
        {
            property.FindPropertyRelative("_useDirectReference").boolValue = booleanValue;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void ValidateAssetAndAssign(AddressablesPathDropdown.Item item, SerializedProperty addressProperty, SerializedProperty mainProperty)
        {
            if (AddressablesPathDictionary.GetInstance().TryGetGUIDFromPath(item.assetPath, out var guid))
            {
                if (!IsAssetValid(guid))
                {
                    Debug.LogWarning($"Addressable asset {item.assetPath} cannot be assigned to {mainProperty.displayName} because it's not valid.");
                    return;
                }

                SetBoolValue(serializedProperty, false);
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

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<BuffDef>(guidOrAddressablePath);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedEliteDef))]
    public sealed class AddressReferencedEliteDefDrawer : AddressReferencedAssetDrawer<AddressReferencedEliteDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the EliteDef";

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<BuffDef>(guidOrAddressablePath);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedEquipmentDef))]
    public sealed class AddressReferencedEquipmentDefDrawer : AddressReferencedAssetDrawer<AddressReferencedEquipmentDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the EquipmentDef";

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<BuffDef>(guidOrAddressablePath);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedExpansionDef))]
    public sealed class AddressReferencedExpansionDefDrawer : AddressReferencedAssetDrawer<AddressReferencedExpansionDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the ExpansionDef";

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<BuffDef>(guidOrAddressablePath);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedItemDef))]
    public sealed class AddressReferencedItemDefDrawer : AddressReferencedAssetDrawer<AddressReferencedItemDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the ItemDef";

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<BuffDef>(guidOrAddressablePath);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedPrefab))]
    public sealed class AddressReferencedPrefabDrawer : AddressReferencedAssetDrawer<AddressReferencedPrefab>
    {

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<GameObject>(guidOrAddressablePath);
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedSpawnCard))]
    public sealed class AddressReferencedSpawnCardDrawer : AddressReferencedAssetDrawer<AddressReferencedSpawnCard>
    {

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<SpawnCard>(guidOrAddressablePath);
        }

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

        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<UnlockableDef>(guidOrAddressablePath);
        }

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
        protected override bool IsAssetValid(string guidOrAddressablePath)
        {
            return DoesAssetInheritFrom<DirectorCardCategorySelection>(guidOrAddressablePath);
        }
    }
#endif
}
#endif