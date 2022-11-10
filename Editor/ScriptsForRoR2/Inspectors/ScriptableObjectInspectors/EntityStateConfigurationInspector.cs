using HG.GeneralSerializer;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public sealed class EntityStateConfigurationInspector : ScriptableObjectInspector<EntityStateConfiguration>, IObjectNameConvention
    {
        public string Prefix => entityStateType == null ? string.Empty : entityStateType.FullName;

        public bool UsesTokenForPrefix => false;

        private delegate object FieldDrawHandler(GUIContent labelTooltip, object value, FieldInfo field);
        private static readonly Dictionary<Type, FieldDrawHandler> typeDrawers = new Dictionary<Type, FieldDrawHandler>
        {
            [typeof(bool)] = (labelTooltip, value, field) => EditorGUILayout.Toggle(labelTooltip, (bool)value),
            [typeof(long)] = (labelTooltip, value, field) => EditorGUILayout.LongField(labelTooltip, (long)value),
            [typeof(int)] = (labelTooltip, value, field) => TryDrawEnumField(labelTooltip, (int)value, field) ?? EditorGUILayout.IntField(labelTooltip, (int)value),
            [typeof(float)] = (labelTooltip, value, field) => EditorGUILayout.FloatField(labelTooltip, (float)value),
            [typeof(double)] = (labelTooltip, value, field) => EditorGUILayout.DoubleField(labelTooltip, (double)value),
            [typeof(string)] = (labelTooltip, value, field) => EditorGUILayout.TextField(labelTooltip, (string)value),
            [typeof(Vector2)] = (labelTooltip, value, field) => EditorGUILayout.Vector2Field(labelTooltip, (Vector2)value),
            [typeof(Vector3)] = (labelTooltip, value, field) => EditorGUILayout.Vector3Field(labelTooltip, (Vector3)value),
            [typeof(Color)] = (labelTooltip, value, field) => EditorGUILayout.ColorField(labelTooltip, (Color)value),
            [typeof(Color32)] = (labelTooltip, value, field) => (Color32)EditorGUILayout.ColorField(labelTooltip, (Color32)value),
            [typeof(AnimationCurve)] = (labelTooltip, value, field) => EditorGUILayout.CurveField(labelTooltip, (AnimationCurve)value ?? new AnimationCurve()),
        };

        private static readonly Dictionary<Type, Func<object>> specialDefaultValueCreators = new Dictionary<Type, Func<object>>
        {
            [typeof(AnimationCurve)] = () => new AnimationCurve(),
        };

        private static readonly ConditionalWeakTable<FieldInfo, Type> enumFieldsCache = new ConditionalWeakTable<FieldInfo, Type>();

        private Type entityStateType;
        private readonly List<FieldInfo> serializableStaticFields = new List<FieldInfo>();
        private readonly List<FieldInfo> serializableInstanceFields = new List<FieldInfo>();

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

                if (typeDrawers.TryGetValue(fieldInfo.FieldType, out var drawer))
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = drawer(guiContent, serializedValue.GetValue(fieldInfo), fieldInfo);

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedValue.SetValue(fieldInfo, newValue);
                        stringValue.stringValue = serializedValue.stringValue;
                    }
                }
                else
                {
                    DrawUnrecognizedField(field);
                }
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
                serializedValue.SetValue(fieldInfo, creator());
            }
            else
            {
                serializedValue.SetValue(fieldInfo, fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : (object)null);
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
                bool canSerialize = SerializedValue.CanSerializeField(fieldInfo);
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

        private static int? TryDrawEnumField(GUIContent labelTooltip, int value, FieldInfo field)
        {
            if (!enumFieldsCache.TryGetValue(field, out var enumType))
            {
                var enumMask = field.GetCustomAttribute<EnumMaskAttribute>();
                enumType = enumMask?.enumType;
                if (!enumType?.IsEnum ?? true)
                {
                    enumFieldsCache.Add(field, null);
                    return null;
                }

                enumFieldsCache.Add(field, enumType);
            }
            else if (enumType == null)
            {
                return null;
            }

            return Convert.ToInt32(EditorGUILayout.EnumFlagsField(labelTooltip, (Enum)Enum.ToObject(enumType, value)));
        }
    }
}