using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.VisualElements
{
    public class ExtendedListView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ExtendedListView, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlIntAttributeDescription m_ListViewItemHeight = new UxmlIntAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(listViewItemHeight)),
                defaultValue = 18
            };

            private UxmlIntAttributeDescription m_ListViewHeight = new UxmlIntAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(baseListViewHeightPixels)),
                defaultValue = 200,
            };

            private UxmlBoolAttributeDescription m_heightHandleBar = new UxmlBoolAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(showHeightHandleBar)),
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_CollectionResizable = new UxmlBoolAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(collectionResizable)),
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_CollectionElementsHaveContextMenus = new UxmlBoolAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(createContextMenuWrappers)),
                defaultValue = false
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ExtendedListView @this = (ExtendedListView)ve;
                @this.listViewItemHeight = m_ListViewItemHeight.GetValueFromBag(bag, cc);
                @this.baseListViewHeightPixels = m_ListViewHeight.GetValueFromBag(bag, cc);
                @this.showHeightHandleBar = m_heightHandleBar.GetValueFromBag(bag, cc);
                @this.collectionResizable = m_CollectionResizable.GetValueFromBag(bag, cc);
                @this.createContextMenuWrappers = m_CollectionElementsHaveContextMenus.GetValueFromBag(bag, cc);
            }
        }

        public bool collectionResizable
        {
            get
            {
                return _collectionResizable;
            }
            set
            {
                _collectionResizable = value;
                collectionSizeField.SetDisplay(value);
                collectionSizeField.SetEnabled(value);
            }
        }
        private bool _collectionResizable;
        public int baseListViewHeightPixels
        {
            get
            {
                return (int)internalListView.style.height.value.value;
            }
            set
            {
                var heightStyle = internalListView.style.height;
                var height = heightStyle.value;
                height.unit = LengthUnit.Pixel;
                height.value = value;

                heightStyle.value = height;
                internalListView.style.height = heightStyle;
            }
        }
        public int listViewItemHeight
        {
            get
            {
                return internalListView.itemHeight;
            }
            set
            {
                internalListView.itemHeight = value;
            }
        }
        public bool createContextMenuWrappers { get; set; }
        public bool showHeightHandleBar
        {
            get
            {
                return _showHeightHandleBar;
            }
            set
            {
                _showHeightHandleBar = value;
                heightHandleBar.SetDisplay(value);
                heightHandleBar.SetEnabled(value);
            }
        }
        private bool _showHeightHandleBar;
        public IntegerField collectionSizeField { get; }
        public VisualElement heightHandleBar { get; }
        private ListView internalListView { get; }
        public SerializedObject serializedObject => _serializedObject;
        private SerializedObject _serializedObject;
        public SerializedProperty collectionProperty
        {
            get => _collectionProperty;
            set
            {
                if(_collectionProperty != value)
                {
                    _collectionProperty = value;
                    _serializedObject = value.serializedObject;
                    SetupCollectionSizeField();
                    SetupListView();
                }
            }
        }
        private SerializedProperty _collectionProperty;
        public Func<VisualElement> CreateElement { get; set; }
        public Action<VisualElement, SerializedProperty> BindElement { get; set; }

        private bool dragHandle = false;
        public void Refresh()
        {
            OnSizeSetInternal(_collectionProperty == null ? 0 : _collectionProperty.arraySize);
        }

        private void SetupCollectionSizeField()
        {
            if (!collectionResizable)
                return;

            collectionSizeField.value = collectionProperty.arraySize;
            collectionSizeField.RegisterValueChangedCallback(OnSizeSet);

            void OnSizeSet(ChangeEvent<int> evt)
            {
                int val = evt.newValue < 0 ? 0 : evt.newValue;
                OnSizeSetInternal(val);
            }
        }
        private void OnSizeSetInternal(int newSize)
        {
            if (collectionResizable)
                collectionSizeField.value = newSize;

            if (collectionProperty != null)
                collectionProperty.arraySize = newSize;

            internalListView.itemsSource = new int[newSize];
            serializedObject?.ApplyModifiedProperties();
        }
        private void SetupListView()
        {
            internalListView.itemsSource = collectionProperty == null ? Array.Empty<int>() : new int[collectionProperty.arraySize];
            internalListView.bindItem = BindItemInternal;
            internalListView.makeItem = MakeItemInternal;
        }

        private VisualElement MakeItemInternal()
        {
            VisualElement element = null;
            if(createContextMenuWrappers)
            {
                element = new ContextualMenuWrapper();
                var userElement = CreateElement();
                userElement.style.flexGrow = new StyleFloat(1f);
                userElement.style.flexShrink = new StyleFloat(0f);
                element.Add(userElement);
                return element;
            }
            return CreateElement();
        }
        private void BindItemInternal(VisualElement ve, int i)
        {
            SerializedProperty propForElement = collectionProperty.GetArrayElementAtIndex(i);
            var visualElementForBinding = (ve is ContextualMenuWrapper wrapper) ? wrapper.contentContainer[0] : ve;
            visualElementForBinding.name = $"element{i}";

            if(createContextMenuWrappers)
            {
                var contextMenuData = new ContextMenuData
                {
                    menuAction = DeleteItem,
                    userData = visualElementForBinding,
                    menuName = "Delete Item",
                    actionStatusCheck = (_) => collectionProperty == null ? DropdownMenuAction.Status.Hidden : DropdownMenuAction.Status.Normal
                };
                visualElementForBinding.AddSimpleContextMenu(contextMenuData);
                contextMenuData.menuAction = DuplicateItem;
                contextMenuData.menuName = "Duplicate Item";
                visualElementForBinding.AddSimpleContextMenu(contextMenuData);
            }
            else
            {
                visualElementForBinding.AddManipulator(new ContextualMenuManipulator(BuildMenu));
            }

            BindElement(visualElementForBinding, propForElement);
            //visualElementForBinding.Bind(serializedObject);
        }

        private void BuildMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete Item", DeleteItem, GetStatus, evt.target);

            DropdownMenuAction.Status GetStatus(DropdownMenuAction _) => collectionProperty != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.None;
        }
        private void DeleteItem(DropdownMenuAction action)
        {
            VisualElement ve = (VisualElement)action.userData;
            string indexAsString = ve.name.Substring("element".Length);
            int index = int.Parse(indexAsString, CultureInfo.InvariantCulture);
            collectionProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            Refresh();
        }
        
        private void DuplicateItem(DropdownMenuAction action)
        {
            VisualElement ve = (VisualElement)action.userData;
            string indexAsString = ve.name.Substring("element".Length);
            int index = int.Parse(indexAsString, CultureInfo.InvariantCulture);
            SerializedProperty propertyAtIndex = collectionProperty.GetArrayElementAtIndex(index);
            propertyAtIndex.DuplicateCommand();
            serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        private void OnAttached(AttachToPanelEvent evt)
        {
            if(showHeightHandleBar)
            {
                heightHandleBar.AddManipulator(new ElementResizerManipulator(internalListView.style, true, false));
            }
        }


        public ExtendedListView()
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, this, (txt) => true);
            collectionSizeField = this.Q<IntegerField>("collectionSize");
            collectionSizeField.isDelayed = true;
            heightHandleBar = this.Q<VisualElement>("resizeBarContainer").Q<VisualElement>("handle");
            internalListView = this.Q<ListView>("listView");
            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }
    }
}
