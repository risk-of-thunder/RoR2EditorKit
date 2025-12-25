using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(ItemDisplayRuleSet))]
    public class ItemDisplayRuleSetInspector : VisualElementScriptableObjectInspector<ItemDisplayRuleSet>
    {
        TextField _keyAssetFilterText;
        ListView _keyAssetListView;
        Button _forceRefreshButton;

        List<SerializedProperty> _filteredListSelection = new List<SerializedProperty>();
        SerializedProperty _keyAssetRuleGroupsProperty;
        int _previousArraySize = -1;
        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            _keyAssetRuleGroupsProperty = serializedObject.FindProperty("keyAssetRuleGroups");
            _previousArraySize = _keyAssetRuleGroupsProperty.arraySize;

            //We need to listen for the filter set, so we can dynamically change our list view depending wether we're filtering or not.
            _keyAssetFilterText = templateInstanceRoot.Q<TextField>("KeyAssetFilter");
            _keyAssetFilterText.RegisterValueChangedCallback(OnFilterSet);

            //Setup the list view itself.
            _keyAssetListView = templateInstanceRoot.Q<ListView>("KeyAssetRuleGroups");
            _keyAssetListView.itemsSource = _filteredListSelection;
            _keyAssetListView.makeItem = CreatePropertyField;
            _keyAssetListView.bindItem = BindPropertyField;
            _keyAssetListView.itemIndexChanged += ElementIndexChanged;
            _keyAssetListView.itemsAdded += ItemsAdded;
            _keyAssetListView.itemsRemoved += ItemsRemoved;

            _forceRefreshButton = templateInstanceRoot.Q<Button>("ForceRefresh");
            _forceRefreshButton.clicked += UpdateListViewToFilter;
            UpdateListViewToFilter();

            //There is no way to listen if someone clicks the "delete array element" context menu of a property field, thanks unity.
            EditorApplication.update += CheckForKeyAssetRuleGroupsPropertyChange;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= CheckForKeyAssetRuleGroupsPropertyChange;
        }

        private void CheckForKeyAssetRuleGroupsPropertyChange()
        {
            try
            {
                if(_previousArraySize != _keyAssetRuleGroupsProperty.arraySize)
                {
                    UpdateListViewToFilter();
                }
            }
            catch(Exception ex)
            {
                EditorApplication.update -= CheckForKeyAssetRuleGroupsPropertyChange;
            }
        }

        //since this only would get called when we're not filtering, we can just be stupid about how we do things.
        private void ItemsRemoved(IEnumerable<int> obj)
        {
            List<int> list = (List<int>)obj;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                _keyAssetRuleGroupsProperty.DeleteArrayElementAtIndex(i);
            }
            _keyAssetRuleGroupsProperty.serializedObject.ApplyModifiedProperties();
            UpdateListViewToFilter();
        }

        private void ItemsAdded(IEnumerable<int> obj)
        {
            int biggest = -1;

            List<int> list = (List<int>)obj;
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i] > biggest)
                {
                    biggest = list[i];
                }
            }

            if(_keyAssetRuleGroupsProperty.arraySize < biggest + 1)
            {
                _keyAssetRuleGroupsProperty.arraySize = biggest + 1;
                _keyAssetRuleGroupsProperty.serializedObject.ApplyModifiedProperties();
                UpdateListViewToFilter();
            }
        }

        private void ElementIndexChanged(int arg1, int arg2)
        {
            _keyAssetRuleGroupsProperty.MoveArrayElement(arg1, arg2);
        }

        private void BindPropertyField(VisualElement element, int arg2)
        {
            PropertyField propField = (PropertyField)element;
            propField.BindProperty(_filteredListSelection[arg2]);
        }

        private VisualElement CreatePropertyField()
        {
            return new PropertyField();
        }

        private void OnFilterSet(ChangeEvent<string> evt)
        {
            string newValue = evt.newValue;

            if(string.IsNullOrWhiteSpace(newValue))
            {
                RestrictListViewCapabilities(isRestricted: false);
            }
            else
            {
                RestrictListViewCapabilities(isRestricted: true);
            }

            UpdateListViewToFilter();
        }

        /*
         * Restricted means that we cant reorder, select, add or modify the collection itself, only interact with the displayed entries.
         */
        private void RestrictListViewCapabilities(bool isRestricted)
        {
            _keyAssetListView.reorderable = isRestricted == false;
            _keyAssetListView.selectionType = isRestricted ? SelectionType.None : SelectionType.Single;
            _keyAssetListView.showFoldoutHeader = isRestricted == false;
            _keyAssetListView.showAddRemoveFooter = isRestricted == false;
            _keyAssetListView.showBoundCollectionSize = isRestricted == false;
        }

        private void UpdateListViewToFilter()
        {
            string newFilter = _keyAssetFilterText.value;

            _filteredListSelection.Clear();

            //We need to check what matches the filter, if there is no filter, add everything.
            for(int i = 0; i < _keyAssetRuleGroupsProperty.arraySize; i++)
            {
                var keyAssetRuleGroup = _keyAssetRuleGroupsProperty.GetArrayElementAtIndex(i);

                if(string.IsNullOrWhiteSpace(newFilter))
                {
                    _filteredListSelection.Add(keyAssetRuleGroup);
                    continue;
                }

                var keyAssetProperty = keyAssetRuleGroup.FindPropertyRelative("keyAsset");
                var keyAssetAddress = keyAssetRuleGroup.FindPropertyRelative("keyAssetAddress");

                //First try to get the KeyAsset directly, check if the name contains the filter.
                if(keyAssetProperty.objectReferenceValue && keyAssetProperty.objectReferenceValue.name.Contains(newFilter))
                {
                    _filteredListSelection.Add(keyAssetRuleGroup);
                }
                else
                {
                    //Otherwise, check the GUID, get the path to the asset, and see if the asset name has the filter.
                    var assetGUID = keyAssetAddress.FindPropertyRelative("m_AssetGUID");
                    string guid = assetGUID.stringValue;

                    if(AddressablesPathDictionary.instance.TryGetPathFromGUID(guid, out var path))
                    {
                        //Malformed paths return null, guard against that.
                        string assetName = System.IO.Path.GetFileName(path);
                        if(string.IsNullOrEmpty(assetName))
                        {
                            continue;
                        }

                        if(assetName.Contains(newFilter))
                        {
                            _filteredListSelection.Add(keyAssetRuleGroup);
                        }
                    }
                }
            }

            //Finally, rebuild.
            _keyAssetListView.Rebuild();
        }
    }
}