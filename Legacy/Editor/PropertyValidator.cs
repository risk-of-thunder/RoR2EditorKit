using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit
{
    /// <summary>
    /// Obsolete, create a custom VisualElement control that inherits from ValidatingField<T>
    /// </summary>
    [Obsolete("Use an element inheriting from ValidatingField<T>, if no element exists for your value, subclass ValidatingField<T> and implement a custom control")]
    public class PropertyValidator<T>
    {
        public enum TiedElementType
        {
            PropertyField,
            INotifyValueChanged,
        }
        public class ActionContainerPair
        {
            public Action action;
            public IMGUIContainer container;
        }

        public VisualElement TiedElement { get; }
        public TiedElementType TypeOfTiedElement { get; }
        public VisualElement ParentElement { get; }
        public ChangeEvent<T> ChangeEvent { get => _changeEvent; }
        private ChangeEvent<T> _changeEvent;

        private Dictionary<Func<bool?>, ActionContainerPair> validatorToMessageAction = new Dictionary<Func<bool?>, ActionContainerPair>();

        public PropertyValidator(PropertyField propField, VisualElement parentElementToAttach)
        {
            TiedElement = propField;
            TypeOfTiedElement = TiedElementType.PropertyField;
            ParentElement = parentElementToAttach;
            TiedElement.RegisterCallback<ChangeEvent<T>>(ValidateInternal);
        }

        public PropertyValidator(INotifyValueChanged<T> valueChangedInterface, VisualElement parentElementToAttach)
        {
            TiedElement = valueChangedInterface as VisualElement;
            TypeOfTiedElement = TiedElementType.INotifyValueChanged;
            ParentElement = parentElementToAttach;
            (TiedElement as INotifyValueChanged<T>).RegisterValueChangedCallback(ValidateInternal);
        }

        public void AddValidator(Func<bool?> condition, string message, MessageType messageType = MessageType.Info)
        {
            validatorToMessageAction.Add(condition, new ActionContainerPair
            {
                action = new Action(() => EditorGUILayout.HelpBox(message, messageType)),
                container = null
            });
        }

        public void ForceValidation() => ValidateInternal(null);
        private void ValidateInternal(ChangeEvent<T> evt)
        {
            _changeEvent = evt;
            foreach (var (validator, actionContainerPair) in validatorToMessageAction)
            {
                bool? value = validator();
                if (value == null)
                {
                    continue;
                }

                if (value.Value)
                {
                    if (actionContainerPair.container == null)
                    {
                        actionContainerPair.container = new IMGUIContainer(actionContainerPair.action);
                        ParentElement.Add(actionContainerPair.container);
                        actionContainerPair.container.BringToFront();
                        continue;
                    }
                    actionContainerPair.container.BringToFront();
                }
                if (actionContainerPair.container != null)
                {
                    actionContainerPair.container.Wipe();
                    actionContainerPair.container.RemoveFromHierarchy();
                    actionContainerPair.container = null;
                }
            }
        }
    }
}