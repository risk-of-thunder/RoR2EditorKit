using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// A <see cref="VisualElement"/> used to display an <see cref="AdvancedDropdown"/> when clicked.
    /// <br></br>
    /// This element inherits from BaseField (Where the generic type is AdvancedDropdownItem). due to this, the usage of bindings is not supported fully.
    /// <br></br>
    /// Keep in mind that on your custom <see cref="AdvancedDropdown"/>'s <see cref="AdvancedDropdown.ItemSelected(AdvancedDropdownItem)"/>, you should call <see cref="OnItemSelected(AdvancedDropdownItem)"/>, this will properly update the VisualElement's UI accordingly and fire a <see cref="ChangeEvent{T}"/>
    /// <para></para>
    /// You need to set <see cref="createDropdown"/>, otherwise the Dropdown Button is disabled. You may also set <see cref="getDropdownButtonText"/> to generate a GUIContent from which the Dropdown Button's label will be set.
    /// </summary>
    public class AdvancedDropdownElement : BaseField<AdvancedDropdownItem>
    {
        /// <summary>
        /// Uxml factory, exposed for the UIBuilder, you shouldnt touch this on code unless you're inheriting from this
        /// </summary>
        public new class UxmlFactory : UxmlFactory<AdvancedDropdownElement, UxmlTraits> { }

        /// <summary>
        /// Uxml Traits, exposed for the UIBuilder, you shouldn't touch this on code unless you're inheriting from this
        /// </summary>
        public new class UxmlTraits : BaseField<AdvancedDropdownItem>.UxmlTraits
        {
            public UxmlStringAttributeDescription m_DropdownButtonText = new UxmlStringAttributeDescription { name = "dropdown-button-text", defaultValue = "Value" };
            public UxmlStringAttributeDescription m_DropdownButtonTooltip = new UxmlStringAttributeDescription { name = "dropdown-button-tooltip", defaultValue = "" };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if (ve is AdvancedDropdownElement ade)
                {
                    ade.dropdownButtonText = m_DropdownButtonText.GetValueFromBag(bag, cc);
                    ade.dropdownButtonTooltip = m_DropdownButtonTooltip.GetValueFromBag(bag, cc);
                }
            }
        }

        /// <summary>
        /// Delegate that creates the AdvancedDropdown to show.
        /// </summary>
        /// <param name="requester">The AdvancedDropdownElement requesting for the AdvancedDropdown itself. Remember that when your AdvancedDropdown calls <see cref="AdvancedDropdown.ItemSelected(AdvancedDropdownItem)"/>, you should class <see cref="AdvancedDropdownElement.OnItemSelected(AdvancedDropdownItem)"/>.</param>
        /// <returns>The created AdvancedDropdown</returns>
        public delegate AdvancedDropdown CreateDropdownDelegate(AdvancedDropdownElement requester);

        /// <summary>
        /// Delegate used to retrieve a GUIContent value from <paramref name="item"/>
        /// </summary>
        /// <param name="item">The item that was selected</param>
        /// <returns>A <see cref="GUIContent"/> that represents <paramref name="item"/></returns>
        public delegate GUIContent GetDropdownButtonTextDelegate(AdvancedDropdownItem item);

        /// <summary>
        /// Event fired when the dropdown button is clicked.
        /// <para>This is exposed for convenience, the AdvancedDropdownElement will show the dropdown created by itself.</para>
        /// </summary>
        public event Action clicked
        {
            add
            {
                if (clickable == null)
                {
                    clickable = new Clickable(value);
                }
                else
                {
                    clickable.clicked += value;
                }
            }
            remove
            {
                if (clickable != null)
                {
                    clickable.clicked -= value;
                }
            }
        }
        /// <summary>
        /// Access to the <see cref="Clickable"/> manipulator for this element
        /// </summary>
        public Clickable clickable
        {
            get => _clickable;
            set
            {
                if (_clickable != null && _clickable.target == _buttonElement)
                {
                    _buttonElement.RemoveManipulator(_clickable);
                }
                _clickable = value;
                if (_clickable != null)
                {
                    _buttonElement.AddManipulator(_clickable);
                }
            }
        }
        private Clickable _clickable;
        /// <summary>
        /// The text value for the label in the dropdown button
        /// </summary>
        public string dropdownButtonText
        {
            get => dropdownButtonLabel.text;
            set => dropdownButtonLabel.text = value;
        }
        public Label dropdownButtonLabel { get; private set; }
        public string dropdownButtonTooltip
        {
            get => _buttonElement.tooltip;
            set => _buttonElement.tooltip = value;
        }
        private VisualElement _buttonElement;

        /// <summary>
        /// Retrieves a string value from an <see cref="AdvancedDropdownItem"/>. Can be left null
        /// </summary>
        public GetDropdownButtonTextDelegate getDropdownButtonText
        {
            get => _onGetDropdownButtonText ?? _defaultGetDropdownButtonText;
            set => _onGetDropdownButtonText = value;
        }
        private GetDropdownButtonTextDelegate _onGetDropdownButtonText = null;
        private static readonly GetDropdownButtonTextDelegate _defaultGetDropdownButtonText = GetDropdownButtonTextDefaultImpl;
        private static GUIContent GetDropdownButtonTextDefaultImpl(AdvancedDropdownItem item)
        {
            return new GUIContent(item?.name ?? "NULL");
        }

        /// <summary>
        /// Used to create the AdvancedDropdown when this element is clicked. Cannot be left null, otherwise the button is disabled.
        /// </summary>
        public CreateDropdownDelegate createDropdown
        {
            get => _createDropdown;
            set
            {
                _createDropdown = value;
                _buttonElement.SetEnabled(_createDropdown != null);
            }
        }
        private CreateDropdownDelegate _createDropdown;

        /// <summary>
        /// Sets a new AdvancedDropdownItem value without notifying
        /// </summary>
        public override void SetValueWithoutNotify(AdvancedDropdownItem newValue)
        {
            base.SetValueWithoutNotify(newValue);
            SetDropdownButtonText(newValue);
        }

        protected override void UpdateMixedValueContent()
        {
            SetDropdownButtonText(value);
        }

        private void SetDropdownButtonText(AdvancedDropdownItem value)
        {
            GUIContent content = getDropdownButtonText(value);
            dropdownButtonText = showMixedValue ? mixedValueString : content.text;
            dropdownButtonTooltip = showMixedValue ? "Mixed Values" : content.tooltip;
        }

        private void OpenDropdown()
        {
            if (createDropdown == null)
                return;

            AdvancedDropdown dropdown = createDropdown?.Invoke(this);
            dropdown.Show(_buttonElement.worldBound);
        }

        /// <summary>
        /// Call this method when <see cref="AdvancedDropdown.ItemSelected(AdvancedDropdownItem)"/> is fired
        /// </summary>
        public void OnItemSelected(AdvancedDropdownItem item)
        {
            value = item;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AdvancedDropdownElement() : this(null, null, null, null)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label's text and tooltip</param>
        /// <param name="currentValue">The current value stored in the <see cref="AdvancedDropdownElement"/></param>
        /// <param name="createDropdownDelegate">The delegate used to create the AdvancedDropdown</param>
        /// <param name="getDropdownButtonTextDelegate">A delegate used to retrieve the GUIContent of the DropdownButton based on the currently selected item</param>
        public AdvancedDropdownElement(GUIContent label, AdvancedDropdownItem currentValue, CreateDropdownDelegate createDropdownDelegate, GetDropdownButtonTextDelegate getDropdownButtonTextDelegate = null) : base(label?.text ?? "Dropdown", VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(AdvancedDropdownElement)))
        {
            dropdownButtonLabel = this.Q<Label>("DropdownLabel");
            _buttonElement = this.Q<VisualElement>("Button");
            _buttonElement.SetEnabled(false);

            clicked += OpenDropdown;

            this.AddToClassList("unity-base-field");
            this.AddToClassList("unity-base-popup-field");
            this.AddToClassList("unity-popup-field");
            this.AddToClassList("unity-base-field__aligned");
            labelElement.tooltip = label?.tooltip ?? string.Empty;
            createDropdown = createDropdownDelegate;
            getDropdownButtonText = getDropdownButtonTextDelegate;

            SetValueWithoutNotify(currentValue);
        }

        ~AdvancedDropdownElement()
        {
        }
    }

}