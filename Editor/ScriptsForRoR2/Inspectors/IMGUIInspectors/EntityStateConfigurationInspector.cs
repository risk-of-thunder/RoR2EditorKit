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
        PropertyValidator<HG.SerializableSystemType> targetTypeValidator;

        public Type EntityStateType
        {
            get
            {
                return _entityStateType;
            }
            private set
            {
                if(_entityStateType != value)
                {
                    _entityStateType = value;
                    PopulateSerializableFields();
                    SetDisplays();
                }
            }
        }
        private Type _entityStateType;

        private List<FieldInfo> serializableStaticFields = new List<FieldInfo>();
        private List<FieldInfo> serializableInstanceFields = new List<FieldInfo>();

        protected override void OnEnable()
        {
            base.OnEnable();

            OnVisualTreeCopy += () =>
            {
                inspectorDataContainer = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                instanceFieldsContainer = inspectorDataContainer.Q<VisualElement>("InstanceFieldsContainer");
                staticFieldsContainer = inspectorDataContainer.Q<VisualElement>("StaticFieldsContainer");
                unrecognizedFieldsContainer = inspectorDataContainer.Q<VisualElement>("UnrecognizedFieldsContainer");
                EntityStateType = (Type)TargetType.targetType;
            };
        }

        protected override void DrawInspectorGUI()
        {
            var targetType = inspectorDataContainer.Q<PropertyField>("targetType");
            SetupValidator(new PropertyValidator<HG.SerializableSystemType>(targetType, DrawInspectorElement));
            targetTypeValidator.ForceValidation();
            targetType.RegisterCallback<ChangeEvent<HG.SerializableSystemType>>(OnEntityStateSet);
        }

        private void OnEntityStateSet(ChangeEvent<HG.SerializableSystemType> evt) => EntityStateType = (Type)evt.newValue;

        private void PopulateSerializableFields()
        {
            serializableInstanceFields.Clear();
            serializableStaticFields.Clear();

            if(EntityStateType == null)
            {
                return;
            }

            var serializableFields = EntityStateType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(fieldInfo =>
                {
                    bool canSerialize = SerializedValue.CanSerializeField(fieldInfo);
                    bool shouldSerialize = !fieldInfo.IsStatic || (fieldInfo.DeclaringType == EntityStateType);
                    bool doesNotHaveAttribute = fieldInfo.GetCustomAttribute<HideInInspector>() == null;

                    return canSerialize && shouldSerialize && doesNotHaveAttribute;
                });

            serializableStaticFields.AddRange(serializableFields.Where(fieldInfo => fieldInfo.IsStatic));
            serializableInstanceFields.AddRange(serializableFields.Where(fieldInfo => !fieldInfo.IsStatic));
        }

        private void SetDisplays()
        {
            instanceFieldsContainer.style.display = serializableInstanceFields.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            staticFieldsContainer.style.display = serializableStaticFields.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetupValidator(PropertyValidator<HG.SerializableSystemType> validator)
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
                Type value = validator.ChangeEvent == null ? ((Type)TargetType.targetType) : (Type)validator.ChangeEvent.newValue;

                return value == null ? String.Empty : value.AssemblyQualifiedName;
            }
        }
    }
}