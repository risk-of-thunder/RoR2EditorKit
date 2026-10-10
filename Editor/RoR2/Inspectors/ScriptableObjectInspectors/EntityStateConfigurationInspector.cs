using HG.GeneralSerializer;
using RoR2.AsyncManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        private SerializedProperty _preloadGUIDReferences;

        protected override void OnEnable()
        {
            base.OnEnable();
            _stateTypeProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.targetType));
            _fieldCollectionProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.serializedFieldsCollection));
            _preloadGUIDReferences = serializedObject.FindProperty(nameof(EntityStateConfiguration.preloadGUIDReferences));
        }

        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            _stateTypeToConfigPropertyField = templateInstanceRoot.Q<PropertyField>("TargetType");
            _stateTypeToConfigPropertyField.TrackPropertyValue(_stateTypeProperty.FindPropertyRelative("assemblyQualifiedName"), (sp) =>
            {
                _serializedFieldCollectionElement.typeBeingSerialized = GetStateType();
                serializedObject.ApplyModifiedProperties();
            });

            _serializedFieldCollectionElement = new SerializedFieldCollectionElement();
            _serializedFieldCollectionElement.boundProperty = _fieldCollectionProperty;
            _serializedFieldCollectionElement.typeBeingSerialized = GetStateType();
            _serializedFieldCollectionElement.onFieldValueChanged = OnFieldValueChanged;
            templateInstanceRoot.Add(_serializedFieldCollectionElement);

            UpdatePreloadGUIDReferences();
        }

        private void OnFieldValueChanged(FieldInfo fieldInfo, SerializedProperty property, object arg3)
        {
            serializedObject.UpdateIfRequiredOrScript();
            if(fieldInfo.FieldType == typeof(EntityStateGameObject))
            {
                UpdatePreloadGUIDReferences();
            }
        }

        private void UpdatePreloadGUIDReferences()
        {
            var serializedFieldArrayProperty = _fieldCollectionProperty.FindPropertyRelative(nameof(SerializedFieldCollection.serializedFields));

            var allSerializableFields = _serializedFieldCollectionElement.serializableFields;
            List<string> preloadGUIDReferencesList = new List<string>();

            foreach(var field in allSerializableFields)
            {
                if(field.FieldType != typeof(EntityStateGameObject))
                {
                    continue;
                }

                SerializedProperty serializedFieldProperty = null;
                for(int i = 0; i < serializedFieldArrayProperty.arraySize; i++)
                {
                    var candidate = serializedFieldArrayProperty.GetArrayElementAtIndex(i);
                    if(candidate.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue == field.Name)
                    {
                        serializedFieldProperty = candidate;
                        break;
                    }
                }

                if (serializedFieldProperty == null)
                    continue;

                SerializedProperty serializedValueProperty = serializedFieldProperty.FindPropertyRelative(nameof(SerializedField.fieldValue));
                preloadGUIDReferencesList.Add(serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue)).stringValue);
            }

            _preloadGUIDReferences.ClearArray();
            _preloadGUIDReferences.arraySize = preloadGUIDReferencesList.Count;
            for (int i = 0; i < preloadGUIDReferencesList.Count; i++)
            {
                var preloadGUIDReferenceProperty = _preloadGUIDReferences.GetArrayElementAtIndex(i);
                preloadGUIDReferenceProperty.stringValue = preloadGUIDReferencesList[i];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Type GetStateType()
        {
            return Type.GetType(_stateTypeProperty.FindPropertyRelative("assemblyQualifiedName").stringValue);
        }
    }
}