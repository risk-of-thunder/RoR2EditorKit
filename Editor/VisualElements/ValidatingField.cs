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
    public abstract class ValidatingField<TValue> : VisualElement, IBindable
    {
        public enum ElementToValidateType
        {
            PropertyField,
            INotifyValueChanged
        }

        public override VisualElement contentContainer => _contentContainer;
        private VisualElement _contentContainer;

        public VisualElement ElementToValidate
        {
            get
            {
                return _elementToValidate;
            }
        }
        private VisualElement _elementToValidate;
        public ElementToValidateType ElementType { get; private set; }
        public ScrollView MessageView { get; }
        public ChangeEvent<TValue> ChangeEvent => _changeEvent;

        public abstract IBinding binding { get; set; }
        public abstract string bindingPath { get; set; }

        private ChangeEvent<TValue> _changeEvent;
        private Dictionary<Func<bool>, HelpBox> validatorToHelpBox = new Dictionary<Func<bool>, HelpBox>();
        public bool SetElementToValidate(VisualElement element)
        {
            if ((element is PropertyField || element is INotifyValueChanged<TValue>) && _elementToValidate != element)
            {
                _elementToValidate?.UnregisterCallback<ChangeEvent<TValue>>(ValidateInternal);
                _elementToValidate = element;
                ElementType = element is PropertyField ? ElementToValidateType.PropertyField : ElementToValidateType.INotifyValueChanged;
                _elementToValidate.RegisterCallback<ChangeEvent<TValue>>(ValidateInternal);
                return true;
            }
            return false;
        }
        public void AddValidator(Func<bool> conditionForMessage, string message, MessageType messageType = MessageType.Info, Action<ContextualMenuPopulateEvent> contextMenu = null)
        {
            if (!validatorToHelpBox.ContainsKey(conditionForMessage))
            {
                validatorToHelpBox.Add(conditionForMessage, new HelpBox(message, messageType, false, contextMenu));
            }
        }
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

        public ValidatingField()
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(nameof(ValidatingField<TValue>), this, (_) => true);
            IStyle internalStyle = style;
            StyleColor borderColor = new StyleColor(new UnityEngine.Color(0.3176471f, 0.3176471f, 0.3176471f));
            internalStyle.SetBorderColor(borderColor);
            internalStyle.SetBorderWidth(new StyleFloat(1f));

            _contentContainer = this.Q<VisualElement>("ContentContainer");
            MessageView = this.Q<ScrollView>("validatingFieldMessages");
        }
    }
}