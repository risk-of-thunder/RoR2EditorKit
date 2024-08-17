using HG.GeneralSerializer;
using RoR2;
using RoR2EditorKit.Inspectors;
using RoR2EditorKit.RoR2Related.PropertyDrawers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;



namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public sealed class EntityStateConfigurationInspector : ScriptableObjectInspector<EntityStateConfiguration>, IObjectNameConvention
    {
        public string Prefix => entityStateType == null ? string.Empty : entityStateType.FullName;

        public bool UsesTokenForPrefix => false;

        private static readonly Dictionary<Type, Func<object>> specialDefaultValueCreators = new Dictionary<Type, Func<object>>
        {
            [typeof(AnimationCurve)] = () => new AnimationCurve(),
        };

        private Type entityStateType;
        private readonly List<FieldInfo> serializableStaticFields = new List<FieldInfo>();
        private readonly List<FieldInfo> serializableInstanceFields = new List<FieldInfo>();

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void DrawInspectorGUI()
        {
            DrawInspectorElement.Clear();
            DrawInspectorElement.Add(new IMGUIContainer(IMGUI));
        }
        private void IMGUI()
        {
            var collectionProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.serializedFieldsCollection));
            var systemTypeProp = serializedObject.FindProperty(nameof(EntityStateConfiguration.targetType));
            var assemblyQuallifiedName = systemTypeProp.FindPropertyRelative("assemblyQualifiedName").stringValue;

            EditorGUILayout.PropertyField(systemTypeProp);

            if (entityStateType?.AssemblyQualifiedName != assemblyQuallifiedName)
            {
                entityStateType = Type.GetType(assemblyQuallifiedName);
                PopulateSerializableFields();
            }

            if (entityStateType == null)
            {
                return;
            }

            var serializedFields = collectionProperty.FindPropertyRelative(nameof(SerializedFieldCollection.serializedFields));

            DrawFields(serializableStaticFields, "Static fields", "There is no static fields");
            DrawFields(serializableInstanceFields, "Instance fields", "There is no instance fields");

            var unrecognizedFields = new List<KeyValuePair<SerializedProperty, int>>();
            for (var i = 0; i < serializedFields.arraySize; i++)
            {
                var field = serializedFields.GetArrayElementAtIndex(i);
                var name = field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue;
                if (!(serializableStaticFields.Any(el => el.Name == name) || serializableInstanceFields.Any(el => el.Name == name)))
                {
                    unrecognizedFields.Add(new KeyValuePair<SerializedProperty, int>(field, i));
                }
            }

            if (unrecognizedFields.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unrecognized fields", EditorStyles.boldLabel);
                if (GUILayout.Button("Clear unrecognized fields"))
                {
                    foreach (var fieldRow in unrecognizedFields.OrderByDescending(el => el.Value))
                    {
                        serializedFields.DeleteArrayElementAtIndex(fieldRow.Value);
                    }
                    unrecognizedFields.Clear();
                }

                EditorGUI.indentLevel++;
                foreach (var fieldRow in unrecognizedFields)
                {
                    DrawUnrecognizedField(fieldRow.Key);
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            void DrawFields(List<FieldInfo> fields, string groupLabel, string emptyLabel)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(groupLabel, EditorStyles.boldLabel);
                if (fields.Count == 0)
                {
                    EditorGUILayout.LabelField(emptyLabel);
                }
                EditorGUI.indentLevel++;
                foreach (var fieldInfo in fields)
                {
                    DrawField(fieldInfo, GetOrCreateField(serializedFields, fieldInfo));
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawUnrecognizedField(SerializedProperty field)
        {
            var name = field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue;
            var valueProperty = field.FindPropertyRelative(nameof(SerializedField.fieldValue));
            EditorGUILayout.PropertyField(valueProperty, new GUIContent(ObjectNames.NicifyVariableName(name)), true);
        }

        private void DrawField(FieldInfo fieldInfo, SerializedProperty field)
        {
            var tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            var guiContent = new GUIContent(ObjectNames.NicifyVariableName(fieldInfo.Name), tooltipAttribute != null ? tooltipAttribute.tooltip : null);

            var serializedValueProperty = field.FindPropertyRelative(nameof(SerializedField.fieldValue));
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
            {
                var objectValue = serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.objectValue));
                EditorGUILayout.ObjectField(objectValue, fieldInfo.FieldType, guiContent);
            }
            else
            {
                var stringValue = serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue));
                var serializedValue = new SerializedValue
                {
                    stringValue = string.IsNullOrWhiteSpace(stringValue.stringValue) ? null : stringValue.stringValue
                };

                DrawFieldUsingDrawers(fieldInfo, field, guiContent, stringValue, ref serializedValue);
            }
        }

        private SerializedProperty GetOrCreateField(SerializedProperty collectionProperty, FieldInfo fieldInfo)
        {
            for (var i = 0; i < collectionProperty.arraySize; i++)
            {
                var field = collectionProperty.GetArrayElementAtIndex(i);
                if (field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue == fieldInfo.Name)
                {
                    return field;
                }
            }
            collectionProperty.arraySize++;

            var serializedField = collectionProperty.GetArrayElementAtIndex(collectionProperty.arraySize - 1);
            var fieldNameProperty = serializedField.FindPropertyRelative(nameof(SerializedField.fieldName));
            fieldNameProperty.stringValue = fieldInfo.Name;

            var fieldValueProperty = serializedField.FindPropertyRelative(nameof(SerializedField.fieldValue));
            var serializedValue = new SerializedValue();
            if (specialDefaultValueCreators.TryGetValue(fieldInfo.FieldType, out var creator))
            {
                SetValue(fieldInfo, ref serializedValue, creator());
            }
            else
            {
                SetValue(fieldInfo, ref serializedValue, fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : (object)null);
            }

            fieldValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue)).stringValue = serializedValue.stringValue;
            fieldValueProperty.FindPropertyRelative(nameof(SerializedValue.objectValue)).objectReferenceValue = null;

            return serializedField;
        }

        private void PopulateSerializableFields()
        {
            serializableStaticFields.Clear();
            serializableInstanceFields.Clear();

            if (entityStateType == null)
            {
                return;
            }

            var allFieldsInType = entityStateType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var filteredFields = allFieldsInType.Where(fieldInfo =>
            {
                bool canSerialize = CanSerialize(fieldInfo);
                bool shouldSerialize = !fieldInfo.IsStatic || (fieldInfo.DeclaringType == entityStateType);
                bool doesNotHaveAttribute = fieldInfo.GetCustomAttribute<HideInInspector>() == null;
                bool notConstant = !fieldInfo.IsLiteral;
                return canSerialize && shouldSerialize && doesNotHaveAttribute && notConstant;
            });

            serializableStaticFields.AddRange(filteredFields.Where(fieldInfo => fieldInfo.IsStatic));
            serializableInstanceFields.AddRange(filteredFields.Where(fieldInfo => !fieldInfo.IsStatic));
        }

        public PrefixData GetPrefixData()
        {
            return new PrefixData
            {
                helpBoxMessage = $"This {GetType().Name}'s name should match the TargetType's FullName so it follows naming conventions",
                contextMenuAction = UpdateName,
                nameValidatorFunc = NameValidator
            };
        }

        private void UpdateName()
        {
            TargetType.SetNameFromTargetType();
            AssetDatabaseUtils.UpdateNameOfObject(TargetType);
        }

        private bool NameValidator()
        {
            Type type = (Type)TargetType.targetType;
            if (type != null)
                return serializedObject.targetObject.name.Equals(type.FullName);
            else
                return true;
        }

        private bool CanSerialize(FieldInfo fieldInfo)
        {
#if RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS
            Type fieldType = fieldInfo.FieldType;
            if(!typeof(UnityEngine.Object).IsAssignableFrom(fieldType) && !EditorStringSerializer.CanSerializeType(fieldType))
            {
                return false;
            }

            if(fieldInfo.IsStatic && fieldInfo.IsPublic)
            {
                return true;
            }
            return fieldInfo.GetCustomAttribute<SerializeField>() != null;
#else
            return SerializedValue.CanSerializeField(fieldInfo);
#endif
        }

        private void DrawFieldUsingDrawers(FieldInfo fieldInfo, SerializedProperty field, GUIContent guiContent, SerializedProperty stringValue, ref SerializedValue serializedValue)
        {
            if (IMGUIUtil.CanDrawFieldFromFieldInfo(fieldInfo))
            {
                if(IMGUIUtil.DrawFieldFromFieldInfo(fieldInfo, GetValue(fieldInfo, ref serializedValue), out var newValue, guiContent))
                {
                    SetValue(fieldInfo, ref serializedValue, newValue);
                    stringValue.stringValue = serializedValue.stringValue;
                }
            }
            else
            {
                DrawUnrecognizedField(field);
            }
        }

        private object GetValue(FieldInfo field, ref SerializedValue serializedValue)
        {
#if RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS
            Type fieldType = field.FieldType;
            if(typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                if((object)serializedValue.objectValue != null)
                {
                    Type type = serializedValue.objectValue.GetType();
                    if(!fieldType.IsAssignableFrom(type))
                    {
                        if(type == typeof(UnityEngine.Object))
                        {
                            return null;
                        }
                    }
                }
                return serializedValue.objectValue;
            }
            if(EditorStringSerializer.CanSerializeType(fieldType) && serializedValue.stringValue != null)
            {
                try
                {
                    return EditorStringSerializer.Deserialize(serializedValue.stringValue, fieldType);
                }
                catch (StringSerializerException ex)
                {
                    Debug.LogWarningFormat("Could not deserialize field '{0}.{1}': {2}", field.DeclaringType.Name, field.Name, ex);
                }
            }
            if (fieldType.IsValueType)
            {
                return Activator.CreateInstance(fieldType);
            }
            return null;
#else
            return serializedValue.GetValue(field);
#endif
        }

        private void SetValue(FieldInfo fieldInfo, ref SerializedValue serializedValue, object newValue)
        {
#if RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS
            try
            {
                serializedValue.stringValue = null;
                serializedValue.objectValue = null;

                if(typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
                {
                    serializedValue.objectValue = (UnityEngine.Object)newValue;
                    return;
                }
                if(EditorStringSerializer.CanSerializeType(fieldInfo.FieldType))
                {
                    serializedValue.stringValue = EditorStringSerializer.Serialize(newValue, fieldInfo.FieldType);
                    return;
                }
                throw new Exception($"Unrecognized type \"{fieldInfo.FieldType.FullName}\".");
            }
            catch(Exception e)
            {
                throw new Exception($"Could not serialize field \"{fieldInfo.DeclaringType.FullName}.{fieldInfo.Name}\"", e);
            }
#else
            serializedValue.SetValue(fieldInfo, newValue);
#endif
        }
    }
}