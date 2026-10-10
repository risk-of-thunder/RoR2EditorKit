using RoR2.AsyncManagement;
using RoR2.Editor.PropertyDrawers;
using System;
using System.Reflection;
using UnityEditor;
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
            private AddressablesPathPropertyDrawerPickerHelper.DrawerArgs _drawerArgs;
            private VisualElementUtil.DeconstructedChangeEvent _changeEvent;

            public EntityStateGameObjectField(VisualElementUtil.ControlBuilderArgs controlBuilderArgs)
            {
                Type requiredComponentType = GetRequiredComponentType(controlBuilderArgs.fieldInfo);
                bool searchComponentInChildren = GetSearchComponentInChildren(controlBuilderArgs.fieldInfo);
                string currentGUIDValue = ((EntityStateGameObject)controlBuilderArgs.valueRetriever()).GetGUID();

                _drawerArgs = new AddressablesPathPropertyDrawerPickerHelper.DrawerArgs
                {
                    allowedTypes = new Type[] { typeof(GameObject) },
                    useFullPathForItems = EditorSettingManager.GetOrCreateSettingsFor(typeof(EntityStateGameObjectField), EditorSettingManager.SettingType.UserSetting).GetOrCreateSetting("useFullPathForItems", true),
                    label = new GUIContent(controlBuilderArgs.elementLabel),
                    requiredComponentType = requiredComponentType,
                    searchComponentInChildren = searchComponentInChildren,
                    currentGUIDValue = currentGUIDValue,
                    onItemSelected = OnItemSelected
                };

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
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2);
                AddressablesPathPropertyDrawerPickerHelper.DrawPicker(rect, _drawerArgs);
            }

            private void OnItemSelected(AddressablesPathDropdown.Item item)
            {
                var oldGUID = _drawerArgs.currentGUIDValue;
                
                if(AddressablesPathDictionary.instance.TryGetGUIDFromPath(item.assetPath, out string guid))
                {
                    _drawerArgs.currentGUIDValue = guid;
                }
                else if(item.isNone)
                {
                    _drawerArgs.currentGUIDValue = "";
                }
                else
                {
                    RoR2EKLog.Warning($"Item {item.assetPath} does not exist within the AddressablesPathDictionary, not applying item.");
                    return;
                }

                _changeEvent(new VisualElementUtil.DeconstructedChangeEventData
                {
                    eventBase = null,
                    newValue = new EntityStateGameObject(_drawerArgs.currentGUIDValue),
                    previousValue = new EntityStateGameObject(oldGUID),
                });
            }
        }
    }
}