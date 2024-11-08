using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public class EntityStateConfigurationInspector : VisualElementScriptableObjectInspector<EntityStateConfiguration>
    {

        private SerializedProperty _stateTypeProperty;
        private PropertyField _stateTypeToConfigPropertyField;

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
            _stateTypeToConfigPropertyField = templateInstanceRoot.Q<PropertyField>();
            _stateTypeToConfigPropertyField.TrackPropertyValue(_stateTypeProperty.FindPropertyRelative("assemblyQualifiedName"), (sp) =>
            {
                _serializedFieldCollectionElement.typeBeingSerialized = GetStateType();
                serializedObject.ApplyModifiedProperties();
            });

            _serializedFieldCollectionElement = new SerializedFieldCollectionElement();
            _serializedFieldCollectionElement.boundProperty = _fieldCollectionProperty;
            _serializedFieldCollectionElement.typeBeingSerialized = GetStateType();
            templateInstanceRoot.Add(_serializedFieldCollectionElement);
        }

        private Type GetStateType()
        {
            return Type.GetType(_stateTypeProperty.FindPropertyRelative("assemblyQualifiedName").stringValue);
        }
    }
}