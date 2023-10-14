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

        private delegate object FieldDrawHandler(GUIContent labelTooltip, object value);
        private static readonly Dictionary<Type, FieldDrawHandler> typeDrawers = new Dictionary<Type, FieldDrawHandler>();

#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
        private static FieldDrawHandler enumFlagsTypeHandler;
        private static FieldDrawHandler enumTypeHandler;
#endif

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
#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
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
#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
            Type fieldType = fieldInfo.FieldType;
            if(typeDrawers.TryGetValue(fieldType, out var drawer))
            {
                EditorGUI.BeginChangeCheck();
                var newValue = drawer(guiContent, GetValue(fieldInfo, ref serializedValue));

                if(EditorGUI.EndChangeCheck())
                {
                    SetValue(fieldInfo, ref serializedValue, newValue);
                    stringValue.stringValue = serializedValue.stringValue;
                }
            }
            else if(fieldType.IsEnum)
            {
                object newValue;
                EditorGUI.BeginChangeCheck();
                if (fieldType.GetCustomAttribute<FlagsAttribute>() != null || fieldType.GetCustomAttribute<EnumMaskAttribute>() != null)
                {
                    newValue = enumFlagsTypeHandler(guiContent, GetValue(fieldInfo, ref serializedValue));
                }
                else
                {
                    newValue = enumTypeHandler(guiContent, GetValue(fieldInfo, ref serializedValue));
                }
                if(EditorGUI.EndChangeCheck())
                {
                    SetValue(fieldInfo, ref serializedValue, newValue);
                    stringValue.stringValue = serializedValue.stringValue;
                }
                return;
            }
            else
            {
                DrawUnrecognizedField(field);
            }
#else
            if (typeDrawers.TryGetValue(fieldInfo.FieldType, out var drawer))
            {
                EditorGUI.BeginChangeCheck();
                var newValue = drawer(guiContent, GetValue(fieldInfo));

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
#endif
        }

        private object GetValue(FieldInfo field, ref SerializedValue serializedValue)
        {
#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
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
            serializedValue.GetValue(field);
#endif
        }

        private void SetValue(FieldInfo fieldInfo, ref SerializedValue serializedValue, object newValue)
        {
#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
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

        static EntityStateConfigurationInspector()
        {
            typeDrawers.Add(typeof(bool), (labelTooltip, value) => EditorGUILayout.Toggle(labelTooltip, (bool)value));
            typeDrawers.Add(typeof(long), (labelTooltip, value) => EditorGUILayout.LongField(labelTooltip, (long)value));
            typeDrawers.Add(typeof(int), (labelTooltip, value) => EditorGUILayout.IntField(labelTooltip, (int)value));
            typeDrawers.Add(typeof(float), (labelTooltip, value) => EditorGUILayout.FloatField(labelTooltip, (float)value));
            typeDrawers.Add(typeof(double), (labelTooltip, value) => EditorGUILayout.DoubleField(labelTooltip, (double)value));
            typeDrawers.Add(typeof(string), (labelTooltip, value) => EditorGUILayout.TextField(labelTooltip, (string)value));
            typeDrawers.Add(typeof(Vector2), (labelTooltip, value) => EditorGUILayout.Vector2Field(labelTooltip, (Vector2)value));
            typeDrawers.Add(typeof(Vector3), (labelTooltip, value) => EditorGUILayout.Vector3Field(labelTooltip, (Vector3)value));
            typeDrawers.Add(typeof(Color), (labelTooltip, value) => EditorGUILayout.ColorField(labelTooltip, (Color)value));
            typeDrawers.Add(typeof(Color32), (labelTooltip, value) => (Color32)EditorGUILayout.ColorField(labelTooltip, (Color32)value));
            typeDrawers.Add(typeof(AnimationCurve), (labelTooltip, value) => EditorGUILayout.CurveField(labelTooltip, (AnimationCurve)value ?? new AnimationCurve()));

#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
            typeDrawers.Add(typeof(LayerMask), (labelTooltip, value) => EditorGUILayout.LayerField(labelTooltip, (LayerMask)value));
            typeDrawers.Add(typeof(Vector4), (labelTooltip, value) => EditorGUILayout.Vector4Field(labelTooltip, (Vector4)value));
            typeDrawers.Add(typeof(Rect), (labelTooltip, value) => EditorGUILayout.RectField(labelTooltip, (Rect)value));
            typeDrawers.Add(typeof(RectInt), (labelTooltip, value) => EditorGUILayout.RectIntField(labelTooltip, (RectInt)value));
            typeDrawers.Add(typeof(char), (labelTooltip, value) =>
            {
                string val = ((char)value).ToString();
                val = EditorGUILayout.TextField(labelTooltip, val);
                return val.ToCharArray().FirstOrDefault();
            });
            typeDrawers.Add(typeof(Bounds), (labelTooltip, value) => EditorGUILayout.BoundsField(labelTooltip, (Bounds)value));
            typeDrawers.Add(typeof(BoundsInt), (labelTooltip, value) => EditorGUILayout.BoundsIntField(labelTooltip, (BoundsInt)value));
            typeDrawers.Add(typeof(Quaternion), (labelTooltip, value) =>
            {
                Quaternion quat = (Quaternion)value;
                Vector3 euler = quat.eulerAngles;
                euler = EditorGUILayout.Vector3Field(labelTooltip, euler);
                return Quaternion.Euler(euler);
            });
            typeDrawers.Add(typeof(Vector2Int), (labelTooltip, value) => EditorGUILayout.Vector2IntField(labelTooltip, (Vector2Int)value));
            typeDrawers.Add(typeof(Vector3Int), (labelTooltip, value) => EditorGUILayout.Vector3IntField(labelTooltip, (Vector3Int)value));
            enumFlagsTypeHandler = (labelTooltip, value) => EditorGUILayout.EnumFlagsField(labelTooltip, (Enum)value);
            enumTypeHandler = (labelTooltip, value) => EditorGUILayout.EnumPopup(labelTooltip, (Enum)value);
#endif
        }
    }
}