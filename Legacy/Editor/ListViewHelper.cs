using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit
{
    /// <summary>
    /// Obsolete, use ExtendedListView instead
    /// </summary>
    [Obsolete("ListViewHelper has been made obsolete by ExtendedListView, use that instead.", false)]
    public class ListViewHelper
    {
        public struct ListViewHelperData
        {
            public SerializedProperty property;
            public ListView listView;
            public IntegerField intField;
            public Action<VisualElement, SerializedProperty> bindElement;
            public Func<VisualElement> createElement;
            public ListViewHelperData(SerializedProperty sp, ListView lv, IntegerField intfld, Func<VisualElement> crtItem, Action<VisualElement, SerializedProperty> bnd)
            {
                property = sp;
                listView = lv;
                intField = intfld;
                bindElement = bnd;
                createElement = crtItem;
            }
        }
        public SerializedObject SerializedObject { get => _serializedObject; }
        private SerializedObject _serializedObject;
        public SerializedProperty SerializedProperty
        {
            get => _serializedProperty;
            set
            {
                if (_serializedProperty != value)
                {
                    _serializedProperty = value;
                    _serializedObject = value.serializedObject;
                    SetupArraySize();
                    SetupListView();
                }
            }
        }
        private SerializedProperty _serializedProperty;
        public ListView TiedListView { get; }
        public IntegerField ArraySize { get; }
        public Action<VisualElement, SerializedProperty> BindElement { get; }
        public Func<VisualElement> CreateElement { get; }
        public ListViewHelper(ListViewHelperData data)
        {
            if (data.property != null)
            {
                _serializedProperty = data.property;
                _serializedObject = SerializedProperty.serializedObject;
            }
            TiedListView = data.listView;
            ArraySize = data.intField;
            BindElement = data.bindElement;
            CreateElement = data.createElement;

            SetupArraySize();
            SetupListView();
        }
        public void Refresh()
        {
            OnSizeSetInternal(SerializedProperty == null ? 0 : SerializedProperty.arraySize);
        }

        private void SetupArraySize()
        {
            ArraySize.value = SerializedProperty == null ? 0 : SerializedProperty.arraySize;
            ArraySize.isDelayed = true;
            ArraySize.RegisterValueChangedCallback(OnSizeSet);

            void OnSizeSet(ChangeEvent<int> evt)
            {
                int value = evt.newValue < 0 ? 0 : evt.newValue;
                OnSizeSetInternal(value);
            }
        }

        private void OnSizeSetInternal(int newSize)
        {
            ArraySize.value = newSize;
            if (SerializedProperty != null)
                SerializedProperty.arraySize = newSize;
            TiedListView.itemsSource = new int[newSize];
            SerializedObject?.ApplyModifiedProperties();
        }
        private void SetupListView()
        {
            TiedListView.itemsSource = SerializedProperty == null ? Array.Empty<int>() : new int[SerializedProperty.arraySize];

            TiedListView.makeItem = CreateElement;
            TiedListView.bindItem = BindItemInternal;
        }
        private void BindItemInternal(VisualElement ve, int i)
        {
            SerializedProperty propForElement = SerializedProperty.GetArrayElementAtIndex(i);
            ve.name = $"element{i}";
            BindElement(ve, propForElement);
            ve.Bind(SerializedObject);
        }
    }
}
