using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    internal sealed class AddressablesPathDictionaryProvider : SettingsProvider
    {
        private AddressablesPathDictionary _dictionary;
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedTypeResultCache;

        private ListView _cacheHitListView;
        private Button _clearCacheButton;

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "RoR2EditorKit", "Addressables" };
            VisualElementTemplateDictionary.instance.DoSave();
            var dictionary = AddressablesPathDictionary.instance;
            dictionary.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            dictionary.SaveDictionary();

            return new AddressablesPathDictionaryProvider("Project/RoR2EditorKit/PerUser/Addressables Path Dictionary", SettingsScope.Project, keywords)
            {
                _dictionary = dictionary,
                _serializedObject = new SerializedObject(dictionary)
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(AddressablesPathDictionary), rootElement);

            _clearCacheButton = rootElement.Q<Button>();
            _cacheHitListView = rootElement.Q<ListView>();

            _cacheHitListView.makeItem = CreateCacheItem;
            _cacheHitListView.bindItem = BindCacheItem;

            SerializedProperty dictionaryCache = _serializedObject.FindProperty("dictionaryCache");
            _serializedTypeResultCache = dictionaryCache.FindPropertyRelative("_serializedTypeResultCache");
            _cacheHitListView.BindProperty(_serializedTypeResultCache);
        }

        private VisualElement CreateCacheItem()
        {
            return VisualElementTemplateDictionary.instance.GetTemplateInstance("AddressablesPathDictionaryCacheEntry");
        }

        private void BindCacheItem(VisualElement item, int index)
        {
            Label typeName = item.Q<Label>("TypeNameValue");
            SerializedProperty cacheKeyHitPair = _serializedTypeResultCache.GetArrayElementAtIndex(index);

            SerializedProperty cacheKey = cacheKeyHitPair.FindPropertyRelative("cacheKey");
            SerializedProperty requiredType = cacheKey.FindPropertyRelative("requiredType");
            SerializedProperty assemblyQualifiedName = requiredType.FindPropertyRelative("assemblyQualifiedName");

            typeName.text = Type.GetType(assemblyQualifiedName.stringValue).FullName;


            Toggle componentInChildren = item.Q<Toggle>("ComponentInChildren");
            SerializedProperty componentInChildrenProperty = cacheKey.FindPropertyRelative("entriesIncludeComponentFoundInChildren");
            componentInChildren.SetEnabled(false);
            componentInChildren.value = componentInChildrenProperty.boolValue;


            Label hitCount = item.Q<Label>("HitCount");
            SerializedProperty cacheHit = cacheKeyHitPair.FindPropertyRelative("cacheHit");
            SerializedProperty pathCache = cacheHit.FindPropertyRelative("pathCache");
            hitCount.text = string.Format("HitCount: {0}", pathCache.arraySize);


            Button deleteButton = item.Q<Button>("DeleteCache");
            deleteButton.clicked += () =>
            {
                _dictionary.dictionaryCache.RemoveCache(Type.GetType(assemblyQualifiedName.stringValue), componentInChildrenProperty.boolValue);
                _cacheHitListView.Rebuild();
            };
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Save();
        }

        private void Save()
        {
            _serializedObject.ApplyModifiedProperties();
            if(_dictionary)
            {
                _dictionary.SaveDictionary();
            }
        }

        public AddressablesPathDictionaryProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }
}
