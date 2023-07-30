using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using UObject = UnityEngine.Object;

namespace RoR2EditorKit.VisualElements
{
    /// <summary>
    /// A ValidatingObjectField is a VisualElement inheriting from <see cref="ValidatingField{TValue}"/> that handles values of type <see cref="UnityEngine.Object"/>, This can also be used as an example on how to create more ValidatingFields for different values.
    /// </summary>
    public class ValidatingObjectField : ValidatingField<UObject>
    {
        //So that the UIBuilder can see the VisualElement
        public new class UxmlFactory : UxmlFactory<ValidatingObjectField, UxmlTraits> { }
        
        //We do not inherit from <see cref="ObjectField.UxmlTraits"/> despite using an ObjectField internally, this is due to the Objectfield traits being private instead of protected, which means we cant set the field's properties to the bag's values.
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(label)),
                defaultValue = "Label"
            };

            private UxmlStringAttributeDescription m_BindingPath = new UxmlStringAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(bindingPath))
            };

            private UxmlBoolAttributeDescription m_AllowSceneObjects = new UxmlBoolAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(allowSceneObjects)),
                defaultValue = true
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ValidatingObjectField field = (ValidatingObjectField)ve;
                field.bindingPath = m_BindingPath.GetValueFromBag(bag, cc);
                field.allowSceneObjects = m_AllowSceneObjects.GetValueFromBag(bag, cc);
                field.label = m_Label.GetValueFromBag(bag, cc);
            }
        }
        public string label { get => objectField.label; set => objectField.label = value; }
        public override IBinding binding { get => objectField.binding; set => objectField.binding = value; }
        public override string bindingPath { get => objectField.bindingPath; set => objectField.bindingPath = value; }
        /// <summary>
        /// Wether the internal ObjectField allows scene objects
        /// </summary>
        public bool allowSceneObjects { get => objectField.allowSceneObjects; set => objectField.allowSceneObjects = value; }
        /// <summary>
        /// The internal <see cref="ObjectField"/> of this ValidatingObjectField
        /// </summary>
        public ObjectField objectField { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidatingObjectField()
        {
            objectField = new ObjectField();
            SetElementToValidate(objectField);
            Add(objectField);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidatingObjectField(Type objectType)
        {
            objectField = new ObjectField();
            objectField.objectType = objectType;
            SetElementToValidate(objectField);
            Add(objectField);
        }
    }
}
