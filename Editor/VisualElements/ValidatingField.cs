using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.VisualElements
{
    /// <summary>
    /// A ValidatingField is a custom VisualElement that works as a replacement for the deprecated <see cref="PropertyValidator{T}"/>
    /// <para>Just like the original PropertyValidator, the ValidatingField element is for creating validation methods for SerializedProperties of an object to ensure these properties are not in what the creator would consider an "Invalid State"</para>
    /// <para>If you want to use a SerializedProperty in the ValidatingField, it is extremely recommended to use <see cref="ValidatingPropertyField"/></para>
    /// <para>Due to it's abstract nature, to create new types of ValidatingField, you need to inherit from this class, an example on how to do this can be found in <see cref="ValidatingObjectField"/></para>
    /// </summary>
    /// <typeparam name="TValue">The type that's used for the internal change event, such as UnityEngine.Object or int</typeparam>
    public abstract class ValidatingField<TValue> : VisualElement, IBindable
    {
        /// <summary>
        /// The type of the VisualElement stored in <see cref="ElementToValidate"/>
        /// </summary>
        public enum ElementToValidateType
        {
            /// <summary>
            /// <see cref="ElementToValidate"/> is of type <see cref="UnityEditor.UIElements.PropertyField"/>
            /// </summary>
            PropertyField,
            /// <summary>
            /// <see cref="ElementToValidate"/> is a VisualElement that implements INotifyValueChanged.
            /// </summary>
            INotifyValueChanged
        }

        public override VisualElement contentContainer => _contentContainer;
        private VisualElement _contentContainer;

        /// <summary>
        /// The VisualElement that's getting the ChangeEvent registered, this VisualElement is either a <see cref="PropertyField"/>, or an element that implements <see cref="INotifyValueChanged{T}"/>
        /// <para>You can know the element type beforehand using <see cref="ElementType"/></para>
        /// </summary>
        public VisualElement ElementToValidate
        {
            get
            {
                return _elementToValidate;
            }
        }
        private VisualElement _elementToValidate;
        /// <summary>
        /// The Type of element thats stored in <see cref="ElementToValidate"/>
        /// <para>Due to limitations on UIToolkit's ChangeEvent, only PropertyFields or classes that implement <see cref="INotifyValueChanged{T}"/> can be validated</para>
        /// </summary>
        public ElementToValidateType ElementType { get; private set; }
        /// <summary>
        /// The scroll view that stores the help boxes created by the validation methodss
        /// </summary>
        public ScrollView MessageView { get; }
        /// <summary>
        /// The latest ChangeEvent that was captured by the ValidatingField, this can be null
        /// </summary>
        public ChangeEvent<TValue> ChangeEvent => _changeEvent;

        public abstract IBinding binding { get; set; }
        public abstract string bindingPath { get; set; }

        private ChangeEvent<TValue> _changeEvent;
        private Dictionary<Func<bool>, HelpBox> validatorToHelpBox = new Dictionary<Func<bool>, HelpBox>();
        /// <summary>
        /// Sets the value of <see cref="ElementToValidate"/>
        /// </summary>
        /// <param name="element">The new element to validate</param>
        /// <returns>True if the set operation was succesful, false otherwise</returns>
        public bool SetElementToValidate(VisualElement element)
        {
            if ((element is PropertyField || element is INotifyValueChanged<TValue>) && _elementToValidate != element)
            {
                _elementToValidate?.UnregisterCallback<ChangeEvent<TValue>>(ValidateInternal);
                if(!(element is PropertyField))
                {
                    element.RegisterCallback<ChangeEvent<TValue>>(ValidateInternal);
                }
                _elementToValidate = element;
                ElementType = element is PropertyField ? ElementToValidateType.PropertyField : ElementToValidateType.INotifyValueChanged;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a new Validation Function to this Validating Field
        /// </summary>
        /// <param name="conditionForMessage">The condition for the validation message to appear, True if the message should appear, false if no message should be displayed</param>
        /// <param name="message">The message for this ValidationFunction, will be placed inside a <see cref="HelpBox"/></param>
        /// <param name="messageType">The severity of the Message, <see cref="MessageType.None"/> is of the least concern</param>
        /// <param name="contextMenu">An Action to create a ContextMenu when the <see cref="HelpBox"/> created by this method is right clicked</param>
        public void AddValidator(Func<bool> conditionForMessage, string message, MessageType messageType = MessageType.Info, Action<ContextualMenuPopulateEvent> contextMenu = null)
        {
            if (!validatorToHelpBox.ContainsKey(conditionForMessage))
            {
                validatorToHelpBox.Add(conditionForMessage, new HelpBox(message, messageType, false, contextMenu));
            }
        }
        /// <summary>
        /// Forces the validation to run, regardless if <see cref="ChangeEvent"/> is null or not
        /// </summary>
        public void ForceValidation() => ValidateInternal(null);
        protected void ValidateInternal(ChangeEvent<TValue> evt)
        {
            if (evt != null)
                _changeEvent = evt;

            foreach(var (validator, helpBox) in validatorToHelpBox)
            {
                bool value = validator();
                if (value && helpBox.parent == null)
                {
                    MessageView.Add(helpBox);
                    continue;
                }
                if (helpBox != null)
                {
                    helpBox.parent?.Remove(helpBox);
                }
            }
            MessageView.SetDisplay(MessageView.contentContainer.childCount > 0);
        }

        /// <summary>
        /// Constructor for ValidatingField
        /// </summary>
        public ValidatingField()
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(nameof(ValidatingField<TValue>), this, VisualElementUtil.ValidateUXMLPath);
            IStyle internalStyle = style;
            StyleColor borderColor = new StyleColor(new UnityEngine.Color(0.3176471f, 0.3176471f, 0.3176471f));
            internalStyle.SetBorderColor(borderColor);
            internalStyle.SetBorderWidth(new StyleFloat(1f));

            _contentContainer = this.Q<VisualElement>("ContentContainer");
            MessageView = this.Q<ScrollView>("validatingFieldMessages");
        }
    }
}