using EntityStates;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public class EntityStateConfigurationInspector : VisualElementScriptableObjectInspector<EntityStateConfiguration>
    {
        public override bool canReuseInstance => true;
        private SerializedProperty _stateTypeProperty;
        private CheckablePropertyField _stateTypeToConfigPropertyField;

        private SerializedProperty _fieldCollectionProperty;
        private SerializedFieldCollectionElement _serializedFieldCollectionElement;

        protected override void OnEnable()
        {
            base.OnEnable();
            _stateTypeProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.targetType));
            _fieldCollectionProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.serializedFieldsCollection));
        }

        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            _stateTypeToConfigPropertyField = templateInstanceRoot.Q<CheckablePropertyField>();
            _stateTypeToConfigPropertyField.bindingPath = _stateTypeProperty.propertyPath;
            _stateTypeToConfigPropertyField.onChangeEventCaught += OnChangeEventCaught;

            _serializedFieldCollectionElement = new SerializedFieldCollectionElement();
            _serializedFieldCollectionElement.boundProperty = _fieldCollectionProperty;
            _serializedFieldCollectionElement.typeBeingSerialized = GetStateType();
            templateInstanceRoot.Add(_serializedFieldCollectionElement);
        }

        private void OnChangeEventCaught(ChangeEvent<object> obj)
        {
            Debug.Log(obj.newValue.GetType().ToString());
            _serializedFieldCollectionElement.typeBeingSerialized = GetStateType();
        }

        private Type GetStateType()
        {
            return Type.GetType(_stateTypeProperty.FindPropertyRelative("assemblyQualifiedName").stringValue);
        }
    }
}