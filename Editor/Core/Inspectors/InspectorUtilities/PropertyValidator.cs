using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using RoR2EditorKit.Utilities;
using UnityEngine;

namespace RoR2EditorKit.Core.Inspectors
{
    public class PropertyValidator<T>
    {
        public class ActionContainerPair
        {
            public Action action;
            public IMGUIContainer container;
        }

        public PropertyField TiedField { get; }
        public VisualElement ParentElement { get; }
        public ChangeEvent<T> ChangeEvent { get => _changeEvent; }
        private ChangeEvent<T> _changeEvent;

        private Dictionary<Func<bool?>, ActionContainerPair> validatorToMessageAction = new Dictionary<Func<bool?>, ActionContainerPair>();

        public PropertyValidator(PropertyField propField, VisualElement parentElementToAttach)
        {
            TiedField = propField;
            ParentElement = parentElementToAttach;
            TiedField.RegisterCallback<ChangeEvent<T>>(ValidateInternal);
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
                if(actionContainerPair.container != null)
                {
                    actionContainerPair.container.Wipe();
                    actionContainerPair.container.RemoveFromHierarchy();
                    actionContainerPair.container = null;
                }
            }
        }
    }
}