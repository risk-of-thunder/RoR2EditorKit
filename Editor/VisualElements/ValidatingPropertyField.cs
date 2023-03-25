using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.VisualElements
{
    public class ValidatingPropertyField : ValidatingField<object>
    {
        private class PropertyFieldWrapper : PropertyField
        {
            public ValidatingPropertyField tie;
            public PropertyFieldWrapper(ValidatingPropertyField tie)
            {
                this.tie = tie;
            }

            public override void HandleEvent(EventBase evt)
            {
                base.HandleEvent(evt);
                Type evtType = evt.GetType();
                if (ReflectionUtils.IsAssignableToGenericType(evtType, typeof(ChangeEvent<>)))
                {
                    ChangeEvent<object> newObjectEvent = ChangeEvent<object>.GetPooled(evtType.GetProperty("previousValue").GetGetMethod().Invoke(evt, new object[0]), evtType.GetProperty("newValue").GetGetMethod().Invoke(evt, new object[0]));
                    newObjectEvent.target = evt.target;
                    tie.ValidateInternal(newObjectEvent);
                }
            }
        }
        public new class UxmlFactory : UxmlFactory<ValidatingPropertyField, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_PropertyPath = new UxmlStringAttributeDescription
            {
                name = nameof(bindingPath)
            };

            private UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = nameof(label)
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ValidatingPropertyField field = (ValidatingPropertyField)ve;
                field.bindingPath = m_PropertyPath.GetValueFromBag(bag, cc);
                field.label = m_Label.GetValueFromBag(bag, cc);
            }
        }
        public override IBinding binding { get => PropertyField.binding; set => PropertyField.binding = value; }
        public override string bindingPath { get => PropertyField.bindingPath; set => PropertyField.bindingPath = value; }
        public string label { get => PropertyField.label; set => PropertyField.label = value; }
        public PropertyField PropertyField { get => _wrapper; }
        private PropertyFieldWrapper _wrapper;

        public ValidatingPropertyField()
        {
            _wrapper = new PropertyFieldWrapper(this);
            SetElementToValidate(PropertyField);
            this.Add(PropertyField);
        }
    }
}