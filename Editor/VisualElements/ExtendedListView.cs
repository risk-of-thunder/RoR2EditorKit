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
    /// <summary>
    /// An ExtendedListView is a custom VisualElement that works as a replacement for the deprecated <see cref="ListViewHelper"/>
    /// <para>The ExtendedListView works as a Wrapper that allows you to create a list view that automatically binds to children in the property specified in <see cref="collectionProperty"/>, Due to not using the ListView's default binding systems, you can create your own elements via the <see cref="CreateElement"/> and the <see cref="BindElement"/> function and action respectively.</para>
    /// <para>The ExtendedListView also contains utilities for working with ContextualMenus created from the <see cref="ContextMenuHelper"/> and contains a Resizable height system.</para>
    /// </summary>
    public class ExtendedListView : VisualElement
    {

        /// <summary>
        /// Instantiates a new ExtendedListView from the data of a UXML file
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ExtendedListView, UxmlTraits> { }

        /// <summary>
        /// UXML traits for the ExtendedListView
        /// </summary>
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

            private UxmlBoolAttributeDescription m_CollectionElementDeleteContextMenu = new UxmlBoolAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(createDeleteContextMenu)),
                defaultValue = true
            };

            private UxmlBoolAttributeDescription m_CollectionElementDuplicateContextMenu = new UxmlBoolAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(createDuplicateContextMenu)),
                defaultValue = true
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
                @this.createDeleteContextMenu = m_CollectionElementDeleteContextMenu.GetValueFromBag(bag, cc);
                @this.createDuplicateContextMenu = m_CollectionElementDuplicateContextMenu.GetValueFromBag(bag, cc);
            }
        }

        /// <summary>
        /// Wether the Collection thats inside the <see cref="collectionProperty"/> can be resized
        /// <para>Setting this to false hides and disables the <see cref="collectionSizeField"/>, making the collection not resizable in the inspector view</para>
        /// </summary>
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
        /// <summary>
        /// The base height of the ListView element in pixels.
        /// </summary>
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

        /// <summary>
        /// The height of each list view item
        /// <inheritdoc cref="ListView.itemHeight"/>
        /// </summary>
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

        /// <summary>
        /// Wether the ExtendedListView will create <see cref="ContextualMenuWrapper"/> so you can add custom ContextMenus to each element.
        /// <para>Setting this to true means that your elements that are created by <see cref="CreateElement"/> will be added to a ContextualMenuWrapper behind the scenes</para>
        /// <para>Setting this to false means that elements wont have a ContextualMenuWrapper, but will still implement the Detele Item and Duplicate Item context menus</para>
        /// </summary>
        public bool createContextMenuWrappers { get; set; }

        public bool createDeleteContextMenu { get; set; }

        public bool createDuplicateContextMenu { get; set; }

        /// <summary>
        /// Wether this ExtendedListView has a resizable height.
        /// <para>When set to false, the Height HandleBar element will be disabled and hidden from view.</para>
        /// </summary>
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

        /// <summary>
        /// The Interger field responsible for changing <see cref="collectionProperty"/>'s <see cref="SerializedProperty.arraySize"/>
        /// <para>If <see cref="collectionResizable"/> is set to False, this element is hidden and disabled.</para>
        /// </summary>
        public IntegerField collectionSizeField { get; }
        /// <summary>
        /// The VisualElement responsible for resizing the height for this ExtendedListView.
        /// <para>If <see cref="showHeightHandleBar"/> is set to False, this element is hidden and disabled.</para>
        /// </summary>
        public VisualElement heightHandleBar { get; }
        private ListView internalListView { get; }
        /// <summary>
        /// The SerializedObject that the <see cref="collectionProperty"/> belongs to.
        /// <para>This is null if <see cref="collectionProperty"/> is null</para>
        /// </summary>
        public SerializedObject serializedObject => _serializedObject;
        private SerializedObject _serializedObject;
        /// <summary>
        /// The SerializedProperty that represetns a collection that this ExtendedListView manages.
        /// <para>Setting this value refreshes the ListView completely and updates it.</para>
        /// </summary>
        public SerializedProperty collectionProperty
        {
            get => _collectionProperty;
            set
            {
                if(_collectionProperty != value)
                {
                    _collectionProperty = value;
                    _serializedObject = value?.serializedObject;
                    SetupCollectionSizeField();
                    SetupListView();
                }
            }
        }
        private SerializedProperty _collectionProperty;
        /// <summary>
        /// A Function that's used to create the VisualElement for an entry in the listView
        /// </summary>
        public Func<VisualElement> CreateElement { get; set; }
        /// <summary>
        /// An Action that's used to bind your VisualElement created in <see cref="CreateElement"/> to the specified SerializedProperty
        /// <para>Some things worth mentioning about the Binding Process:</para>
        /// <para>1. the VisualElement argument is always the VisualElement that was created in <see cref="CreateElement"/></para>
        /// <para>2. The VisualElement argument has its name changed to "elementX", where X is it's array index in the <see cref="_collectionProperty"/>'s internal array.</para>
        /// <para>3. Depending on the value of <see cref="createContextMenuWrappers"/>, if it's set to True, the VisualElement argument has a parent, and said parent is the <see cref="ContextualMenuWrapper"/>, otherwise the VisualElement argument has no parent.</para>
        /// <para>4. The <see cref="ExtendedListView"/> takes care of calling the Bind() method on your VisualElement.</para>
        /// </summary>
        public Action<VisualElement, SerializedProperty> BindElement { get; set; }

        private bool dragHandle = false;

        /// <summary>
        /// Completely refreshes the ExtendedListView
        /// </summary>
        public void Refresh()
        {
            SetupListView();
            SetupCollectionSizeField();
            OnSizeSetInternal(_collectionProperty == null ? 0 : _collectionProperty.arraySize);
        }

        private void SetupCollectionSizeField()
        {
            if (!collectionResizable)
                return;

            collectionSizeField.value = collectionProperty?.arraySize ?? internalListView.itemsSource.Count;
            collectionSizeField.UnregisterValueChangedCallback(OnSizeSet);
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
                if(createDeleteContextMenu)
                {
                    var contextMenuData = new ContextMenuData
                    {
                        menuAction = DeleteItem,
                        userData = visualElementForBinding,
                        menuName = "Delete Item",
                        actionStatusCheck = (_) => collectionProperty == null ? DropdownMenuAction.Status.Hidden : DropdownMenuAction.Status.Normal
                    };
                    visualElementForBinding.AddSimpleContextMenu(contextMenuData);
                }
                if(createDuplicateContextMenu)
                {
                    var contextMenuData = new ContextMenuData
                    {
                        menuAction = DuplicateItem,
                        userData = visualElementForBinding,
                        menuName = "Duplicate Item",
                        actionStatusCheck = (_) => collectionProperty == null ? DropdownMenuAction.Status.Hidden : DropdownMenuAction.Status.Normal
                    };
                    visualElementForBinding.AddSimpleContextMenu(contextMenuData);
                }
            }
            else
            {
                visualElementForBinding.AddManipulator(new ContextualMenuManipulator(BuildMenu));
            }

            BindElement(visualElementForBinding, propForElement);
            visualElementForBinding.Bind(serializedObject);
        }

        private void BuildMenu(ContextualMenuPopulateEvent evt)
        {
            if(createDeleteContextMenu)
            {
                evt.menu.AppendAction("Delete Item", DeleteItem, GetStatus, evt.target);
            }
            if(createDuplicateContextMenu)
            {
                evt.menu.AppendAction("Duplicate Item", DuplicateItem, GetStatus, evt.target);
            }

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

        /// <summary>
        /// Constructor for the ExtendedListView.
        /// </summary>
        public ExtendedListView()
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, this, VisualElementUtil.ValidateUXMLPath);
            collectionSizeField = this.Q<IntegerField>("collectionSize");
            collectionSizeField.isDelayed = true;
            heightHandleBar = this.Q<VisualElement>("resizeBarContainer").Q<VisualElement>("handle");
            internalListView = this.Q<ListView>("listView");
            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }

        /// <summary>
        /// Constructor for ExtendedListView
        /// </summary>
        public ExtendedListView(int baseListViewHeightPixels, int listViewItemHeight, bool collectionResizable, bool createContextMenuWrappers, bool heightHandleBar)
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, this, VisualElementUtil.ValidateUXMLPath);
            collectionSizeField = this.Q<IntegerField>("collectionSize");
            collectionSizeField.isDelayed = true;
            this.heightHandleBar = this.Q<VisualElement>("resizeBarContainer").Q<VisualElement>("handle");
            internalListView = this.Q<ListView>("listView");

            this.baseListViewHeightPixels = baseListViewHeightPixels;
            this.listViewItemHeight = listViewItemHeight;
            this.collectionResizable = collectionResizable;
            this.createContextMenuWrappers = createContextMenuWrappers;
            this.showHeightHandleBar = heightHandleBar;

            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }

        /// <summary>
        /// Constructor for ExtendedListView
        /// </summary>
        public ExtendedListView(int baseListViewHeightPixels, int listViewItemHeight, bool collectionResizable, bool createContextMenuWrappers, bool heightHandleBar, SerializedProperty collectionProperty)
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, this, VisualElementUtil.ValidateUXMLPath);
            collectionSizeField = this.Q<IntegerField>("collectionSize");
            collectionSizeField.isDelayed = true;
            this.heightHandleBar = this.Q<VisualElement>("resizeBarContainer").Q<VisualElement>("handle");
            internalListView = this.Q<ListView>("listView");

            this.baseListViewHeightPixels = baseListViewHeightPixels;
            this.listViewItemHeight = listViewItemHeight;
            this.collectionResizable = collectionResizable;
            this.createContextMenuWrappers = createContextMenuWrappers;
            this.showHeightHandleBar = heightHandleBar;

            this.collectionProperty = collectionProperty;

            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }
    }
}
