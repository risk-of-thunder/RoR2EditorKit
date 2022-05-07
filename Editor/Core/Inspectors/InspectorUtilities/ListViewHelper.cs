using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.Inspectors
{
    public class ListViewHelper
    {
        public struct ListViewHelperData
        {
            public SerializedObject serializedObject;
            public SerializedProperty property;
            public ListView listView;
            public IntegerField intField;
            public Action<VisualElement, SerializedProperty> bindElement;
            public Func<VisualElement> createElement;

            public ListViewHelperData(SerializedObject so, SerializedProperty sp, ListView lv, IntegerField intfld, Action<VisualElement, SerializedProperty> bnd, Func<VisualElement> crtItem)
            {
                serializedObject = so;
                property = sp;
                listView = lv;
                intField = intfld;
                bindElement = bnd;
                createElement = crtItem;
            }
        }

        public SerializedObject SerializedObject { get; }
        public SerializedProperty SerializedProperty { get; }
        public ListView TiedListView { get; }
        public IntegerField ArraySize { get; }
        public Action<VisualElement, SerializedProperty> BindElement { get; }
        public Func<VisualElement> CreateElement;

        public ListViewHelper(ListViewHelperData data)
        {
            SerializedObject = data.serializedObject;
            SerializedProperty = data.property;
            TiedListView = data.listView;
            ArraySize = data.intField;
            BindElement = data.bindElement;
            CreateElement = data.createElement;

            SetupArraySize();
            SetupListView();
        }
        private void SetupArraySize()
        {
            ArraySize.value = SerializedProperty.arraySize;
            ArraySize.isDelayed = true;
            ArraySize.RegisterValueChangedCallback(OnSizeSet);

            void OnSizeSet(ChangeEvent<int> evt)
            {
                int value = evt.newValue < 0 ? 0 : evt.newValue;
                ArraySize.value = value;
                SerializedProperty.arraySize = value;
                TiedListView.itemsSource = new int[value];
                SerializedObject.ApplyModifiedProperties();
            }
        }
        private void SetupListView()
        {
            TiedListView.itemsSource = new int[SerializedProperty.arraySize];

            //Ensures height is never 0
            if(TiedListView.style.height.value.value <= 0)
            {
                TiedListView.style.height = 100f;
            }

            TiedListView.makeItem = CreateElement;
            TiedListView.bindItem = BindItemInternal;
        }
        private void BindItemInternal(VisualElement ve, int i)
        {
            SerializedProperty propForElement = SerializedProperty.GetArrayElementAtIndex(i);
            BindElement(ve, propForElement);
            ve.Bind(SerializedObject);
        }
    }
}
