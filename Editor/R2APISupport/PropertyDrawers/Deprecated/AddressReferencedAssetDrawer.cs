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
    [Obsolete("You should inherit from the non-generic version of AddressReferencedAssetDrawer<T>")]
    public abstract class AddressReferencedAssetDrawer<T> : IMGUIPropertyDrawer<T> where T : AddressReferencedAsset
    {
        private static bool _useFullPathForItems;

        protected virtual string AddressTooltip { get; } = "The Address to the Asset";

        protected bool usingDirectReference;

        protected bool canLoadFromCatalog;

        private string _filter;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var _assetProperty = property.FindPropertyRelative("_asset");
            var _addressProperty = property.FindPropertyRelative("_address");
            var _useDirectReferenceProperty = property.FindPropertyRelative("_useDirectReference");
            var _canLoadFromCatalogProperty = property.FindPropertyRelative("_canLoadFromCatalog");

            usingDirectReference = _useDirectReferenceProperty.boolValue;
            canLoadFromCatalog = _canLoadFromCatalogProperty?.boolValue ?? false;

            EditorGUI.BeginProperty(position, label, property);
            var fieldRect = new Rect(position.x, position.y, position.width - 16, standardPropertyHeight);
            var width = GetWidthForSnugLabel(property.GetGUIContent());

            var rectForFilterProperty = new Rect(fieldRect.x + width, fieldRect.y + standardPropertyHeight, fieldRect.width - width, standardPropertyHeight);


            string assetName = null;
            if(AddressablesPathDictionary.instance.TryGetPathFromGUID(_addressProperty.stringValue, out var path))
            {
                assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            }

            if(usingDirectReference)
            {
                EditorGUI.PropertyField(fieldRect, _assetProperty, new GUIContent(property.displayName));
            }
            else
            {
                _filter = EditorGUI.TextField(rectForFilterProperty, "Filter:", _filter);
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

        protected virtual void ModifyContextMenu(GenericMenu menu) { }

        private void OpenAddressablesDropdown(Rect rectForDropdown, SerializedProperty addressProperty)
        {
            AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new UnityEditor.IMGUI.Controls.AdvancedDropdownState(), _useFullPathForItems, _filter, GetRequiredAssetType());
            dropdown.onItemSelected += (item) =>
            {
                ValidateAssetAndAssign(item, addressProperty);
            };
            dropdown.Show(rectForDropdown);
        }

        protected virtual Type GetRequiredAssetType()
        {
            //Get the type of the field, and a reference to AddressReferencedAsset<>
            Type fieldInfoType = fieldInfo.FieldType;
            Type addressReferencedAssetT = typeof(AddressReferencedAsset<>);

            Type genericTypeDefinition = fieldInfoType.IsGenericType ? fieldInfoType.GetGenericTypeDefinition() : null;
            while(genericTypeDefinition != addressReferencedAssetT)
            {
                //We did not get the generic type definition match, go to the base type and try getting it.
                fieldInfoType = fieldInfoType.BaseType;
                if(fieldInfoType.BaseType == null)
                {
                    throw new NullReferenceException($"The type of {fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name} does not inherit from AddressReferencedAsset<>");
                }
                genericTypeDefinition = fieldInfoType.IsGenericType ? fieldInfoType.GetGenericTypeDefinition() : null;
            }

            //We reached the actual AddressReferencedAsset, get the type from it.
            Type result = null;
            Type[] genericArguments = fieldInfoType.GetGenericArguments();
            if(genericArguments.Length > 0)
            {
                result = genericArguments[0];
            }

            if(result == null)
            {
                RoR2EKLog.Warning($"Could not automatically obtain the asset type for AddressReferencedAsset<> in {fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name}");
            }
            return result;
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

        public AddressReferencedAssetDrawer()
        {
            _useFullPathForItems = propertyDrawerPreferenceSettings.GetOrCreateSetting("useFullNameForItems", false);
        }
    }
}
#endif