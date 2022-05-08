using HG.GeneralSerializer;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using RoR2EditorKit.Utilities;
using EntityStates;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public sealed class EntityStateConfigurationInspector : ScriptableObjectInspector<EntityStateConfiguration>
    {

        VisualElement inspectorDataContainer;
        VisualElement instanceFieldsContainer;
        VisualElement staticFieldsContainer;
        VisualElement unrecognizedFieldsContainer;
        PropertyValidator<string> targetTypeValidator;

        private Type entityStateType;

        protected override void OnEnable()
        {
            base.OnEnable();
            entityStateType = (Type)TargetType.targetType;

            OnVisualTreeCopy += () =>
            {
                inspectorDataContainer = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                instanceFieldsContainer = inspectorDataContainer.Q<VisualElement>("InstanceFieldsContainer");
                staticFieldsContainer = inspectorDataContainer.Q<VisualElement>("StaticFieldsContainer");
                unrecognizedFieldsContainer = inspectorDataContainer.Q<VisualElement>("UnrecognizedFieldsContainer");
            };
        }

        protected override void DrawInspectorGUI()
        {
            var targetType = inspectorDataContainer.Q<PropertyField>("targetType");
            SetupValidator(new PropertyValidator<string>(targetType, DrawInspectorElement));
            targetTypeValidator.ForceValidation();
        }
        private void SetupValidator(PropertyValidator<string> validator)
        {
            targetTypeValidator = validator;
            validator.AddValidator(() =>
            {
                var tt = GetTargetType();
                return tt.IsNullOrEmptyOrWhitespace();
            },
            $"Target Type is Null, Empty or Whitespace", MessageType.Warning);

            validator.AddValidator(() =>
            {
                var tt = Type.GetType(GetTargetType());
                if(tt == null)
                    return null;

                return tt.IsAbstract;
            },
            $"Cannot configure an EntityState that's abstract", MessageType.Error);

            validator.AddValidator(() =>
            {
                var tt = Type.GetType(GetTargetType());
                if (tt == null)
                    return null;

                return !tt.IsSubclassOf(typeof(EntityState));
            },
            $"Cannot configure a type that doesnt inherit from EntityState", MessageType.Error);

            string GetTargetType()
            {
                Type value = validator.ChangeEvent == null ? ((Type)TargetType.targetType) : Type.GetType(validator.ChangeEvent.newValue);

                return value == null ? String.Empty : value.AssemblyQualifiedName;
            }
        }
    }
}