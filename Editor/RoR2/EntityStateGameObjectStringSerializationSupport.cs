using RoR2.AsyncManagement;
using RoR2.Editor.PropertyDrawers;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

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
            private AdvancedDropdownElement _advancedDropdownElement;
            private TextField _filterElement;

            private FieldInfo _targetFieldInfo;
            private string _filter;
            private bool _useFullPathForItems;
            private VisualElementUtil.DeconstructedChangeEvent _changeEvent;

            public EntityStateGameObjectField(VisualElementUtil.ControlBuilderArgs controlBuilderArgs)
            {
                VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(EntityStateGameObjectField), this);

                _advancedDropdownElement = this.Q<AdvancedDropdownElement>();
                _advancedDropdownElement.label = controlBuilderArgs.elementLabel;
                _advancedDropdownElement.getDropdownButtonText = GetDropdownButtonLabel;
                _advancedDropdownElement.createDropdown = CreateDropdown;
                _advancedDropdownElement.value = GetCurrentValue(controlBuilderArgs.valueRetriever());
                _advancedDropdownElement.RegisterValueChangedCallback(OnValueChanged);

                _filterElement = this.Q<TextField>();
                _filterElement.RegisterValueChangedCallback((evt) => _filter = evt.newValue);

                var editorSettingCollection = EditorSettingManager.GetOrCreateSettingsFor(GetType(), EditorSettingManager.SettingType.UserSetting);
                _useFullPathForItems = editorSettingCollection.GetOrCreateSetting("useFullPathForItems", false);

                _targetFieldInfo = controlBuilderArgs.fieldInfo;
                _changeEvent = controlBuilderArgs.changeEvent;
            }

            private void OnValueChanged(ChangeEvent<AdvancedDropdownItem> evt)
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

            private AddressablesPathDropdown.Item GetCurrentValue(object currentValueBoxed)
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