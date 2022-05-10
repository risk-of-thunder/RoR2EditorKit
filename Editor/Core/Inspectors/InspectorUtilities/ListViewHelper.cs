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
    /// <summary>
    /// A wrapper for ListView for ease of use.
    /// <para>The wrapper allows the end user to create a Listview that automatically binds to children in the property given by its constructor.</para>
    /// <para>Unlike normally setting the Listview's bindingPath and relying on that, the ListViewHelper allows for extra modification of the elements created and bound.</para>
    /// <para>ListViewHelper also always ensures the ListView's style height is never 0</para>
    /// <para>For usage, look at RoR2EK's NetworkStateMachineInspector</para>
    /// </summary>
    public class ListViewHelper
    {
        /// <summary>
        /// Data for initializing a ListViewHelper
        /// </summary>
        public struct ListViewHelperData
        {
            internal SerializedProperty property;
            internal ListView listView;
            internal IntegerField intField;
            internal Action<VisualElement, SerializedProperty> bindElement;
            internal Func<VisualElement> createElement;

            /// <summary>
            /// ListViewHelperData Constructor
            /// </summary>
            /// <param name="sp">The SerializedProperty thats going to be displayed using the ListView, Property must be an Array Property</param>
            /// <param name="lv">The ListView element</param>
            /// <param name="intfld">An IntegerField that's used for modifying the SerializedProperty's ArraySize</param>
            /// <param name="crtItem">Function for creating a new Element to display</param>
            /// <param name="bnd">Action for binding the SerializedProperty to the VisualElement, there is no need to call Bind() on any elements, as the ListViewHelper takes care of it.</param>
            /// <exception cref="InvalidOperationException">Thrown when the <paramref name="sp"/> is not an Array Property</exception>
            public ListViewHelperData(SerializedProperty sp, ListView lv, IntegerField intfld, Func<VisualElement> crtItem, Action<VisualElement, SerializedProperty> bnd)
            {
                if(sp.isArray)
                {
                    throw new InvalidOperationException($"Cannot create a ListViewHelperData with a SerializedProperty ({sp.name}) thats not an Array property!");
                }
                property = sp;
                listView = lv;
                intField = intfld;
                bindElement = bnd;
                createElement = crtItem;
            }
        }

        /// <summary>
        /// The SerializedObject that owns the <see cref="SerializedProperty"/>
        /// </summary>
        public SerializedObject SerializedObject { get; }
        /// <summary>
        /// The SerializedProperty thats being used for the ListView
        /// </summary>
        public SerializedProperty SerializedProperty { get; }
        /// <summary>
        /// The ListView element
        /// </summary>
        public ListView TiedListView { get; }
        /// <summary>
        /// An IntegerField thats used for modifying the <see cref="SerializedProperty"/>'s ArraySize
        /// </summary>
        public IntegerField ArraySize { get; }
        /// <summary>
        /// The Action for Binding a VisualElement
        /// </summary>
        public Action<VisualElement, SerializedProperty> BindElement { get; }
        /// <summary>
        /// The Function for creating the VisualElement
        /// </summary>
        public Func<VisualElement> CreateElement { get; }

        /// <summary>
        /// ListViewHelper Constructor
        /// </summary>
        /// <param name="data">The Data for constructiong the ListView</param>
        public ListViewHelper(ListViewHelperData data)
        {
            SerializedObject = data.property.serializedObject;
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
