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
    /// <summary>
    /// A ValidatingPropertyField is a VisualElement inheriting from <see cref="ValidatingField{TValue}"/> that handles any value that a PropertyField might use.
    /// <para>Due to not being able to create Generic Visual Elements from the UIBuilder, the ValidatingPropertyField's TValue is of <see cref="System.Object"/>, make sure to cast to your required type when using <see cref="ValidatingField{TValue}.ChangeEvent"/></para>
    /// </summary>
    public class ValidatingPropertyField : ValidatingField<object>
    {
        //This is a fucking hack and i'm impressed it even works
        private class PropertyFieldWrapper : PropertyField
        {
            public ValidatingPropertyField tie;
            public PropertyFieldWrapper(ValidatingPropertyField tie)
            {
                this.tie = tie;
            }

            //Note to self, we cant explicitly know the generic's type of ChangeEvent, so we have to use some Reflection magic to create a new ChangeEvent of type object so the ValidatingPropertyField can validate.
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
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(bindingPath)),
                obsoleteNames = new List<string>
                {
                    "bindingPath",
                }
            };

            private UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(label))
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
        /// <summary>
        /// The label of the property field
        /// </summary>
        public string label { get => PropertyField.label; set => PropertyField.label = value; }
        /// <summary>
        /// The property field that this ValidatingPropertyField has
        /// </summary>
        public PropertyField PropertyField { get => _wrapper; }
        private PropertyFieldWrapper _wrapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidatingPropertyField()
        {
            _wrapper = new PropertyFieldWrapper(this);
            SetElementToValidate(PropertyField);
            this.Add(PropertyField);
        }
    }
}