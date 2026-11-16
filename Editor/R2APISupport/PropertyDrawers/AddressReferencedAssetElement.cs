using R2API.AddressReferencedAssets;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RoR2.Editor.VisualElements
{
    public class AddressReferencedAssetElement : VisualElement
    {
        public TextField textFieldAddressFilter { get; private set; }
        public AdvancedDropdownElement advancedDropdownElementAddress { get; private set; }
        public TextField textFieldAddress { get; private set; }
        public ObjectField objectFieldDirectReference { get; private set; }
        private VisualElement _addressControls;

        public Type assetType
        {
            get => _assetType;
            set
            {
                //Ensure only types inheriting or being Object are allowed
                Type unityObjectType = typeof(Object);
                if(IsSameOrSubclassOf(value, unityObjectType))
                {
                    _assetType = value;
                    objectFieldDirectReference.objectType = value;
                }
            }
        }
        private Type _assetType;
        public string address
        {
            get => _addressProperty?.stringValue ?? _address;
            set
            {
                //If the value is not null, and we're forbiddenm from loading from a catalog.
                if(!string.IsNullOrWhiteSpace(value) && !canLoadFromCatalog)
                {
                    //Reject if guid is invalid
                    if(!AddressablesPathDictionary.instance.IsGUIDValid(value))
                    {
                        RoR2EKLog.Error($"Cannot set {value} as an address, as it didn't resolve to an entry within the AddressablesPathDictionary");
                        return;
                    }

                    //Reject if type is null
                    Type guidType = AddressablesPathDictionary.instance.GetTypeFromGUID(value);
                    if(guidType == null)
                    {
                        RoR2EKLog.Error($"Cannot set {value} as an address, as it's type is null.");
                        return;
                    }

                    //Reject if type doesnt match
                    if(!IsSameOrSubclassOf(guidType, assetType))
                    {
                        RoR2EKLog.Error($"Cannot set {value} as an address, as it's type is not {assetType} or a subclass of it.");
                        return;
                    }
                }

                bool changed = false;
                if(_addressProperty != null && _addressProperty.stringValue != value)
                {
                    _addressProperty.stringValue = value;
                    _addressProperty.serializedObject.ApplyModifiedProperties();
                    changed = true;
                }
                if(_address != value)
                {
                    _address = value;
                    changed = true;
                }

                //Address is not null, nullify asset
                if(!string.IsNullOrWhiteSpace(address))
                {
                    asset = null;
                }

                UpdateControlsDependingOnValues();

                if (changed)
                {

                }
            }
        }
        private string _address;
        private SerializedProperty _addressProperty;

        public Object asset
        {
            get
            {
                if(_assetProperty != null && _assetProperty.objectReferenceValue)
                {
                    return _assetProperty.objectReferenceValue;
                }
                return _asset;
            }
            set
            {
                if (value && !IsSameOrSubclassOf(value.GetType(), assetType))
                {
                    RoR2EKLog.Error($"Cannot assign asset {value} as it's not a type of {assetType} or a subclass of it.");
                    return;
                }

                bool changed = false;
                if (_assetProperty != null && _assetProperty.objectReferenceValue != value)
                {
                    _assetProperty.objectReferenceValue = value;
                    _assetProperty.serializedObject.ApplyModifiedProperties();
                    changed = true;
                }
                if (_asset != value)
                {
                    _asset = value;
                    changed = true;
                }

                //Asset is not null, nullify address
                if(asset)
                {
                    address = null;
                }

                UpdateControlsDependingOnValues();

                if (changed)
                {

                }
            }
        }
        private Object _asset;
        private SerializedProperty _assetProperty;

        public bool useDirectReference
        {
            get => _useDirectReferenceProperty?.boolValue ?? _useDirectReference;
            set
            {
                bool changed = false;
                if (_useDirectReferenceProperty != null && _useDirectReferenceProperty.boolValue != value)
                {
                    _useDirectReferenceProperty.boolValue = value;
                    _useDirectReferenceProperty.serializedObject.ApplyModifiedProperties();
                    changed = true;
                }
                if (_useDirectReference != value)
                {
                    _useDirectReference = value;
                    changed = true;
                }

                //If we are now using direct reference, null the address, otherwise, null the asset
                if (useDirectReference)
                {
                    address = null;
                }
                else
                {
                    asset = null;
                }

                UpdateControlsDependingOnValues();

                if (changed)
                {

                }
            }
        }
        private bool _useDirectReference;
        private SerializedProperty _useDirectReferenceProperty;

        public bool canLoadFromCatalog
        {
            get
            {
                //If we have the attribute, return false
                if (_catalogUsageForbidden)
                    return false;

                return _canLoadFromCatalogProperty?.boolValue ?? _canLoadFromCatalog;
            }
            set
            {
                //If we have the attribute, force to false
                if(_catalogUsageForbidden)
                {
                    _canLoadFromCatalog = false;

                    if (_canLoadFromCatalogProperty != null)
                        _canLoadFromCatalogProperty.boolValue = false;

                    return;
                }

                bool changed = false;
                if(_canLoadFromCatalogProperty != null && _canLoadFromCatalogProperty.boolValue != value)
                {
                    _canLoadFromCatalogProperty.boolValue = value;
                    _canLoadFromCatalogProperty.serializedObject.ApplyModifiedProperties();
                    changed = true;
                }
                if(_canLoadFromCatalog != value)
                {
                    _canLoadFromCatalog = value;
                    changed = true;
                }

                UpdateControlsDependingOnValues();

                if (changed)
                {

                }
            }
        }
        private bool _canLoadFromCatalog;
        private SerializedProperty _canLoadFromCatalogProperty;
        public void ForbidCatalogUsage(bool value)
        {
            _catalogUsageForbidden = value;
            if(value)
            {
                canLoadFromCatalog = false;
            }
        }
        private bool _catalogUsageForbidden;

        public Type[] assetTypesForDropdown { get; set; }

        public Type addressableComponentRequirement { get; set; }
        public bool searchInChildren { get; set; }

        private bool _useFullPathForItems;
        private string _filter;

        public void BindProperty(SerializedProperty property)
        {
            if(property == null)
            {
                _canLoadFromCatalogProperty = null;
                _addressProperty = null;
                _assetProperty = null;
                _useDirectReferenceProperty = null;

                advancedDropdownElementAddress.label = "Address Referenced Asset";
                textFieldAddress.label = "Address Referenced Asset";
                objectFieldDirectReference.label = "Address Referenced Asset";

                UpdateControlsDependingOnValues();
                return;
            }

            SerializedProperty assetProperty = property.FindPropertyRelative("_asset");
            SerializedProperty addressProperty = property.FindPropertyRelative("_address");
            SerializedProperty useDirectReferenceProperty = property.FindPropertyRelative("_useDirectReference");
            SerializedProperty canLoadFromCatalogProperty = property.FindPropertyRelative("_canLoadFromCatalog");

            if(assetProperty == null || addressProperty == null || useDirectReferenceProperty == null)
            {
                RoR2EKLog.Error($"Cannot bind property {property.propertyPath} to {nameof(AddressReferencedAssetElement)} as it doesnt match the property structure of a {nameof(AddressReferencedAsset<Object>)}");
                return;
            }

            GUIContent content = property.GetGUIContent();
            advancedDropdownElementAddress.SetLabelGUIContent(content);
            textFieldAddress.SetLabelGUIContent(content);
            objectFieldDirectReference.SetLabelGUIContent(content);

            _canLoadFromCatalogProperty = canLoadFromCatalogProperty;
            _useDirectReferenceProperty = useDirectReferenceProperty;
            _assetProperty = assetProperty;
            _addressProperty = addressProperty;

            UpdateControlsDependingOnValues();
        }

        bool _isUpdatingControls = false;
        private void UpdateControlsDependingOnValues()
        {
            if (_isUpdatingControls)
                return;

            _isUpdatingControls = true;
            try
            {
                //Enable or disable control display depending on values
                if (useDirectReference)
                {
                    _addressControls.SetDisplay(false);
                    advancedDropdownElementAddress.SetDisplay(false);
                    textFieldAddress.SetDisplay(false);
                    objectFieldDirectReference.SetDisplay(true);
                }
                else
                {
                    _addressControls.SetDisplay(true);
                    advancedDropdownElementAddress.SetDisplay(canLoadFromCatalog == false);
                    textFieldAddress.SetDisplay(canLoadFromCatalog == true);
                    objectFieldDirectReference.SetDisplay(false);
                }

                //If cant load from catalog is true, and the address is invalid, set address to null
                if (!canLoadFromCatalog && !AddressablesPathDictionary.instance.IsGUIDValid(address))
                {
                    address = null;
                }

                if (string.IsNullOrWhiteSpace(address))
                {
                    textFieldAddress.value = "";
                    advancedDropdownElementAddress.SetValueWithoutNotify(new AddressablesPathDropdown.Item("None", "", "", true));
                }
                else
                {
                    textFieldAddress.value = address;

                    string tooltip = "";
                    string text = "";

                    if (AddressablesPathDictionary.instance.IsGUIDValid(address))
                    {
                        var path = AddressablesPathDictionary.instance.GetPathFromGUID(address);

                        text = path.Substring(path.LastIndexOf("/") + 1);
                        tooltip = path;
                    }
                    advancedDropdownElementAddress.dropdownButtonText = text;
                    advancedDropdownElementAddress.dropdownButtonTooltip = tooltip;
                }

                objectFieldDirectReference.value = asset;

                MarkDirtyRepaint();
            }
            finally
            {
                _isUpdatingControls = false;
            }
        }

        private bool IsSameOrSubclassOf(Type a, Type b)
        {
            return a == b || (a?.IsSubclassOf(b) ?? false);
        }

        public AddressReferencedAssetElement(bool useFullPathForItems, bool? useDirectReference) : this(useFullPathForItems, useDirectReference, null)
        {
        }

        public AddressReferencedAssetElement(bool useFullPathForItems, bool? useDirectReferenceValue, SerializedProperty targetAddressReferencedAssetProperty)
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(AddressReferencedAssetElement), this);

            var _rootElement = this.Q<VisualElement>("RootElement");

            _addressControls = _rootElement.Q<VisualElement>("AddressControls");
            advancedDropdownElementAddress = _rootElement.Q<AdvancedDropdownElement>();
            textFieldAddressFilter = _rootElement.Q<TextField>("Filter");
            textFieldAddress = _rootElement.Q<TextField>();
            objectFieldDirectReference = _rootElement.Q<ObjectField>();

            //Setup the basic elements
            textFieldAddress.RegisterValueChangedCallback((evt) => address = evt.newValue);
            textFieldAddressFilter.RegisterValueChangedCallback((evt) => _filter = evt.newValue);
            objectFieldDirectReference.RegisterValueChangedCallback((evt) => asset = evt.newValue);

            //Set the asset type to default
            assetType = typeof(UnityEngine.Object);

            //Setup the dropdown
            advancedDropdownElementAddress.createDropdown = (_) =>
            {
                var dropdown = new AddressablesPathDropdown(new AdvancedDropdownState(), _useFullPathForItems, _filter, addressableComponentRequirement, searchInChildren, assetTypesForDropdown);
                dropdown.onItemSelected += advancedDropdownElementAddress.OnItemSelected;
                return dropdown;
            };
            advancedDropdownElementAddress.getDropdownButtonText = (value) =>
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
            };
            advancedDropdownElementAddress.RegisterValueChangedCallback(value =>
            {
                if(value.newValue is AddressablesPathDropdown.Item item)
                {
                    address = item.assetGUID;
                }
            });

            //Use direct reference switch
            _rootElement.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Use Direct Reference",
                actionStatusCheck = (act) => useDirectReference ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal,
                menuAction = (act) => useDirectReference = !useDirectReference
            });
            //Can load from catalog switch
            _rootElement.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Can Load from Catalog",
                actionStatusCheck = (act) =>
                {
                    if (_catalogUsageForbidden || useDirectReference)
                        return DropdownMenuAction.Status.Hidden;

                    return canLoadFromCatalog ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                },
                menuAction = (act) =>
                {
                    if (_catalogUsageForbidden)
                    {
                        canLoadFromCatalog = false;
                        return;
                    }
                    canLoadFromCatalog = !canLoadFromCatalog;
                }
            });
            //Open address picker
            _rootElement.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Open Address Picker",
                actionStatusCheck = (act) => (canLoadFromCatalog && !useDirectReference) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden,
                menuAction = (act) =>
                {
                    var dropdown = advancedDropdownElementAddress.createDropdown?.Invoke(advancedDropdownElementAddress);
                    dropdown.Show(textFieldAddress.worldBound);
                }
            });

            if(useDirectReferenceValue.HasValue)
            {
                this.useDirectReference = useDirectReferenceValue.Value;
            }

            if(targetAddressReferencedAssetProperty != null)
            {
                BindProperty(targetAddressReferencedAssetProperty);
            }
        }
    }
}