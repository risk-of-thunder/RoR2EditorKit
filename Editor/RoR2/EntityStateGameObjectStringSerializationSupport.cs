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
            private VisualElementUtil.DeconstructedChangeEvent _changeEvent;

            private Type _requiredComponentType;
            private bool _searchComponentInChildren;
            private string _filter;
            private string _currentGUIDValue;
            private bool _useFullPathForItems;
            private GUIContent _elementLabel;

            public EntityStateGameObjectField(VisualElementUtil.ControlBuilderArgs controlBuilderArgs)
            {
                _requiredComponentType = GetRequiredComponentType(controlBuilderArgs.fieldInfo);
                _searchComponentInChildren = GetSearchComponentInChildren(controlBuilderArgs.fieldInfo);
                _currentGUIDValue = ((EntityStateGameObject)controlBuilderArgs.valueRetriever()).GetGUID();
                _useFullPathForItems = EditorSettingManager.GetOrCreateSettingsFor(typeof(EntityStateGameObjectField), EditorSettingManager.SettingType.UserSetting).GetOrCreateSetting("useFullPathForItems", true);
                _elementLabel = new GUIContent(controlBuilderArgs.elementLabel);
                _changeEvent = controlBuilderArgs.changeEvent;

                Add(new IMGUIContainer(DrawProperty));
            }

            private bool GetSearchComponentInChildren(FieldInfo fInfo)
            {
                if (fInfo == null)
                    return false;

#if R2EK_R2API_ADDRESSABLES
                return fInfo.GetCustomAttribute<R2API.AddressReferencedAssets.AddressableComponentRequirementAttribute>()?.searchInChildren ?? false;
#else
                return false;
#endif
            }

            private Type GetRequiredComponentType(FieldInfo fInfo)
            {
                if (fInfo == null)
                    return null;

#if R2EK_R2API_ADDRESSABLES
                return fInfo.GetCustomAttribute<R2API.AddressReferencedAssets.AddressableComponentRequirementAttribute>()?.requiredComponentType ?? null;
#else
                return null;
#endif
            }

            private void DrawProperty()
            {
                Rect totalRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2);

                //Rect for the field itself
                Rect fieldRect = new Rect(totalRect.x, totalRect.y, totalRect.width, EditorGUIUtility.singleLineHeight);

                var rectForDropdownButton = EditorGUI.PrefixLabel(fieldRect, _elementLabel);

                var filterRect = new Rect(fieldRect.x + 32, fieldRect.y + EditorGUIUtility.singleLineHeight, fieldRect.width - 32, EditorGUIUtility.singleLineHeight);
                _filter = EditorGUI.TextField(filterRect, "Filter:", _filter);

                GUIContent dropdownButtonLabel = GetDropdownButtonLabel();
                if(EditorGUI.DropdownButton(rectForDropdownButton, dropdownButtonLabel, FocusType.Passive))
                {
                    AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new AdvancedDropdownState(),
                        _useFullPathForItems,
                        _filter,
                        _requiredComponentType,
                        _searchComponentInChildren,
                        typeof(GameObject));

                    dropdown.onItemSelected += OnItemSelected;
                    dropdown.Show(rectForDropdownButton);
                }
            }

            private void OnItemSelected(AddressablesPathDropdown.Item item)
            {
                var oldGUID = _currentGUIDValue;
                
                if(AddressablesPathDictionary.instance.TryGetGUIDFromPath(item.assetPath, out string guid))
                {
                    _currentGUIDValue = guid;
                }
                else if(item.isNone)
                {
                    _currentGUIDValue = "";
                }
                else
                {
                    RoR2EKLog.Warning($"Item {item.assetPath} does not exist within the AddressablesPathDictionary, not applying item.");
                    return;
                }

                _changeEvent(new VisualElementUtil.DeconstructedChangeEventData
                {
                    eventBase = null,
                    newValue = new EntityStateGameObject(_currentGUIDValue),
                    previousValue = new EntityStateGameObject(oldGUID),
                });
            }

            private GUIContent GetDropdownButtonLabel()
            {
                string label = "";
                string tooltip = "";

                if (string.IsNullOrEmpty(_currentGUIDValue))
                {
                    label = "None";
                    tooltip = "";
                    return new GUIContent(label, tooltip);
                }

                string fullAssetPath = AddressablesPathDictionary.instance.GetPathFromGUID(_currentGUIDValue);
                string onlyAssetName = fullAssetPath.Substring(fullAssetPath.LastIndexOf('/') + 1);

                label = onlyAssetName;
                tooltip = fullAssetPath;

                return new GUIContent(label, tooltip);
            }
        }
    }
}