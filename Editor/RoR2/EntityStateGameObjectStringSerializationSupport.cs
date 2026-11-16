using HG.GeneralSerializer;
using RoR2.AsyncManagement;
using RoR2.Editor.PropertyDrawers;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RoR2.Editor.VisualElementUtil;

namespace RoR2.Editor
{
    internal static class EntityStateGameObjectStringSerializationSupport
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorStringSerializer.AddSerializationHandler<EntityStateGameObject>(txt =>
            {
                return new EntityStateGameObject(txt);
            }, 
            obj =>
            {
                if(obj is EntityStateGameObject esgo)
                {
                    return esgo.GetGUID();
                }
                return string.Empty;
            });

            SerializationMediator.AddTypeToSerialize<EntityStateGameObject>();

            VisualElementUtil.AddControlBuilderForType<EntityStateGameObject>(EntityStateGameObjectControlBuilder);
        }

        private static VisualElement EntityStateGameObjectControlBuilder(VisualElementUtil.ControlBuilderArgs args)
        { 
            return new EntityStateGameObjectField(args);
        }

        private class EntityStateGameObjectField : VisualElement
        {
            enum UIState
            {
                Address,
                Reference
            }
            private string _label;
            private Func<object> _valueRetriever;
            private SerializedProperty _fieldProperty;
            private FieldInfo _targetFieldInfo;

            private VisualElement _addressPickerContainer;
            private AdvancedDropdownElement _advancedDropdownElement;
            private TextField _filterElement;

            private VisualElement _objectFieldContainer;
            private ObjectField _objectField;
            private bool _directReferenceCheckmark;

            private string _filter;
            private bool _useFullPathForItems;
            private VisualElementUtil.DeconstructedChangeEvent _changeEvent;

            public EntityStateGameObjectField(VisualElementUtil.ControlBuilderArgs controlBuilderArgs)
            {
                _fieldProperty = controlBuilderArgs.fieldProperty;
                _targetFieldInfo = controlBuilderArgs.fieldInfo;
                _changeEvent = controlBuilderArgs.changeEvent;
                _label = controlBuilderArgs.elementLabel;
                _valueRetriever = controlBuilderArgs.valueRetriever;

                VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(EntityStateGameObjectField), this);

                InitializeElements();
                
                UpdateUIWithState(GetCurrentUIState());

#if R2EK_R2API_STRINGSERIALIZER
                InitializeDirectReferenceSupport(_addressPickerContainer.parent);
#else
                _objectFieldContainer.SetDisplay(false);
                _objectFieldContainer.SetEnabled(false);
#endif

                var editorSettingCollection = EditorSettingManager.GetOrCreateSettingsFor(GetType(), EditorSettingManager.SettingType.UserSetting);
                _useFullPathForItems = editorSettingCollection.GetOrCreateSetting("useFullPathForItems", false);
            }

            private void InitializeElements()
            {
                object value = _valueRetriever?.Invoke();

                AddressablesPathDropdown.Item currentPickerValue = GetCurrentDropdownValue(value);
                GameObject currentDirectReferenceValue = GetCurrentDirectReferenceValue(value);
                _directReferenceCheckmark = currentDirectReferenceValue != null;

                //Addressable part
                _addressPickerContainer = this.Q<VisualElement>("AddressPicker");

                _advancedDropdownElement = _addressPickerContainer.Q<AdvancedDropdownElement>();
                _advancedDropdownElement.label = _label;
                _advancedDropdownElement.getDropdownButtonText = GetDropdownButtonLabel;
                _advancedDropdownElement.createDropdown = CreateDropdown;
                _advancedDropdownElement.value = currentPickerValue;
                _advancedDropdownElement.RegisterValueChangedCallback(OnAddressReferenceValueChanged);

                _filterElement = _addressPickerContainer.Q<TextField>();
                _filterElement.RegisterValueChangedCallback((evt) => _filter = evt.newValue);

                //Direct Reference part
                _objectFieldContainer = this.Q<VisualElement>("ObjectField");
                _objectField = _objectFieldContainer.Q<ObjectField>();
                _objectField.label = _label;
                _objectField.SetObjectType<GameObject>();
                _objectField.value = currentDirectReferenceValue;
                _objectField.RegisterValueChangedCallback(OnDirectReferenceValueChanged);
            }

            private GameObject GetCurrentDirectReferenceValue(object value)
            {
                if(value is GameObject go)
                {
                    return go;
                }
                return null;
            }

            private void UpdateUIWithState(UIState uiState)
            {
                _objectFieldContainer.SetDisplay(uiState == UIState.Reference);
                _objectFieldContainer.SetEnabled(uiState == UIState.Reference);

                _addressPickerContainer.SetDisplay(uiState == UIState.Address);
                _addressPickerContainer.SetEnabled(uiState == UIState.Address);
            }

            private UIState GetCurrentUIState()
            {
#if R2EK_R2API_STRINGSERIALIZER
                if(_directReferenceCheckmark)
                {
                    return UIState.Reference;
                }
                SerializedProperty objectValue = _fieldProperty.FindPropertyRelative("fieldValue").FindPropertyRelative("objectValue");
                return objectValue.objectReferenceValue ? UIState.Reference : UIState.Address;
#else
                return UIState.Address;
#endif
            }

            private void InitializeDirectReferenceSupport(VisualElement contextMenuTarget)
            {
                contextMenuTarget.AddSimpleContextMenu(new ContextMenuData
                {
                    menuName = "Use Direct Reference",
                    actionStatusCheck = (act) => GetCurrentUIState() == UIState.Address ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Checked,
                    menuAction = (act) =>
                    {
                        if(_directReferenceCheckmark == true)
                        {
                            _objectField.value = null;
                            _directReferenceCheckmark = false;
                        }
                        else
                        {
                            _advancedDropdownElement.value = GetCurrentDropdownValue(null);
                            _directReferenceCheckmark = true;
                        }
                        UpdateUIWithState(GetCurrentUIState());
                    }
                });
            }

            private void OnDirectReferenceValueChanged(ChangeEvent<UnityEngine.Object> evt)
            {
                _changeEvent(new DeconstructedChangeEventData
                {
                    eventBase = evt,
                    newValue = evt.newValue,
                    previousValue = evt.previousValue
                });
            }

            private void OnAddressReferenceValueChanged(ChangeEvent<AdvancedDropdownItem> evt)
            {
                var previousValue = evt.previousValue;
                var newValue = evt.newValue;

                if (previousValue is not AddressablesPathDropdown.Item previousItem || newValue is not AddressablesPathDropdown.Item newItem)
                {
                    return;
                }

                var oldGUID = previousItem.assetGUID;
                string newGUID = "";
                if(newItem.isValid)
                {
                    newGUID = newItem.assetGUID;
                }
                else if(newItem.isNone)
                {
                    newGUID = "";
                }
                else
                {
                    RoR2EKLog.Warning($"Item {newItem.assetPath} does not exist within the AddressablesPathDictionary, not applying item.");
                    return;
                }

                _changeEvent(new VisualElementUtil.DeconstructedChangeEventData
                {
                    eventBase = evt,
                    newValue = new EntityStateGameObject(newGUID),
                    previousValue = new EntityStateGameObject(oldGUID)
                });
            }

            private bool GetSearchComponentInChildren()
            {
                if (_targetFieldInfo == null)
                    return false;

#if R2EK_R2API_ADDRESSABLES
                return _targetFieldInfo.GetCustomAttribute<R2API.AddressReferencedAssets.AddressableComponentRequirementAttribute>()?.searchInChildren ?? false;
#else
                return false;
#endif
            }

            private Type GetRequiredComponentType()
            {
                if (_targetFieldInfo == null)
                    return null;

#if R2EK_R2API_ADDRESSABLES
                return _targetFieldInfo.GetCustomAttribute<R2API.AddressReferencedAssets.AddressableComponentRequirementAttribute>()?.requiredComponentType ?? null;
#else
                return null;
#endif
            }

            private AdvancedDropdown CreateDropdown(AdvancedDropdownElement requester)
            {
                var dropdown = new AddressablesPathDropdown(new AdvancedDropdownState(),
                    _useFullPathForItems,
                    _filter,
                    GetRequiredComponentType(),
                    GetSearchComponentInChildren(),
                    typeof(GameObject));
                dropdown.onItemSelected += requester.OnItemSelected;
                return dropdown;
            }

            private AddressablesPathDropdown.Item GetCurrentDropdownValue(object currentValueBoxed)
            {
                var none = new AddressablesPathDropdown.Item("None", "None", "", true);
                if (currentValueBoxed is not EntityStateGameObject esgo)
                {
                    return none;
                }

                string guid = esgo.GetGUID();
                if(!AddressablesPathDictionary.instance.IsGUIDValid(guid))
                {
                    return none;
                }

                var path = AddressablesPathDictionary.instance.GetPathFromGUID(guid);
                return new AddressablesPathDropdown.Item(_useFullPathForItems ? path : path.Substring(path.LastIndexOf("/") + 1), path, guid);
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
        }
    }
}