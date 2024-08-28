using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public class SerializedFieldCollectionElement : VisualElement
    {
        private static readonly Dictionary<Type, Func<object>> _specialDefaultValueCreators = new Dictionary<Type, Func<object>>
        {
            [typeof(AnimationCurve)] = () => new AnimationCurve(),
            [typeof(Gradient)] = () => new Gradient(),
        };

        public Type typeBeingSerialized
        {
            get
            {
                return _typeBeingSerialized;
            }
            set
            {
                if (_typeBeingSerialized != value)
                {
                    _typeBeingSerialized = value;
                    CheckForTypeBeingSerialized();
                }
            }
        }
        private Type _typeBeingSerialized;
        public UnityEngine.UIElements.HelpBox helpBox { get; private set; }
        public VisualElement container { get; private set; }
        public Foldout staticFieldsFoldout { get; private set; }
        public Foldout instanceFieldsFoldout { get; private set; }

        public VisualElement unrecognizedFieldContainer { get; private set; }
        public Button clearUnrecognizedFieldsButton { get; private set; }
        public Foldout unrecognizedFieldsFoldout { get; private set; }
        public SerializedProperty boundProperty
        {
            get
            {
                return _boundProperty;
            }
            set
            {
                if (_boundProperty != value)
                {
                    _boundProperty = value;
                    _serializedFieldsProperty = _boundProperty.FindPropertyRelative("serializedFields");
                }
            }
        }
        private SerializedProperty _boundProperty;

        private SerializedProperty _serializedFieldsProperty;
        private readonly List<FieldInfo> _serializableStaticFields = new List<FieldInfo>();
        private readonly List<FieldInfo> _serializableInstanceFields = new List<FieldInfo>();
        private readonly List<KeyValuePair<SerializedProperty, int>> _unrecognizedFields = new List<KeyValuePair<SerializedProperty, int>>();
        public void CheckForTypeBeingSerialized()
        {
            if (typeBeingSerialized == null)
            {
                container.SetEnabled(false);
                helpBox.SetDisplay(true);
            }
            else
            {
                container.SetEnabled(true);
                helpBox.SetDisplay(false);
            }

            PopulateSerializableFields();

            UpdateSerializedFieldElements();

            CheckAndDrawUnrecognizedFields();
        }

        private void PopulateSerializableFields()
        {
            _serializableStaticFields.Clear();
            _serializableInstanceFields.Clear();

            if (typeBeingSerialized == null)
                return;

            var allFieldsInType = typeBeingSerialized.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var filteredFields = allFieldsInType.Where(fInfo =>
            {
                bool canSerialize = SerializationMediator.CanSerializeField(fInfo);
                bool shouldSerialize = !fInfo.IsStatic || (fInfo.DeclaringType == typeBeingSerialized);
                bool doesNotHaveAttribute = fInfo.GetCustomAttribute<NonSerializedAttribute>() == null;
                bool notConstant = !fInfo.IsLiteral;
                return canSerialize && shouldSerialize && doesNotHaveAttribute && notConstant;
            });

            _serializableInstanceFields.AddRange(filteredFields.Where(fInfo => !fInfo.IsStatic));
            _serializableStaticFields.AddRange(filteredFields.Where(fInfo => fInfo.IsStatic));
        }

        private void UpdateSerializedFieldElements()
        {

            instanceFieldsFoldout.Clear();
            if (_serializableInstanceFields.Count == 0)
            {
                instanceFieldsFoldout.Add(new Label($"There are no instance fields..."));
            }
            else
            {
                foreach (var fieldInfo in _serializableInstanceFields)
                {
                    var control = CreateControl(fieldInfo, GetOrCreateField(_serializedFieldsProperty, fieldInfo));
                    instanceFieldsFoldout.Add(control);
                }
            }

            staticFieldsFoldout.Clear();
            if (_serializableStaticFields.Count == 0)
            {
                staticFieldsFoldout.Add(new Label($"There are no sstatic fields..."));
            }
            else
            {
                foreach (var fieldInfo in _serializableStaticFields)
                {
                    var control = CreateControl(fieldInfo, GetOrCreateField(_serializedFieldsProperty, fieldInfo));
                    staticFieldsFoldout.Add(control);
                }
            }
        }

        private void CheckAndDrawUnrecognizedFields()
        {
            unrecognizedFieldsFoldout.Clear();
            _unrecognizedFields.Clear();

            for (int i = 0; i < _serializedFieldsProperty.arraySize; i++)
            {
                var fieldProperty = _serializedFieldsProperty.GetArrayElementAtIndex(i);
                var name = fieldProperty.FindPropertyRelative("fieldName").stringValue;
                if (!(_serializableStaticFields.Any(el => el.Name == name) || _serializableInstanceFields.Any(el => el.Name == name)))
                {
                    _unrecognizedFields.Add(new KeyValuePair<SerializedProperty, int>(fieldProperty, i));
                }
            }

            if (_unrecognizedFields.Count <= 0)
            {
                unrecognizedFieldContainer.SetDisplay(false);
                unrecognizedFieldContainer.SetEnabled(false);
                unrecognizedFieldsFoldout.Clear();
                return;
            }

            unrecognizedFieldContainer.SetDisplay(true);
            unrecognizedFieldContainer.SetEnabled(true);

            foreach (var fieldRow in _unrecognizedFields)
            {
                DrawUnrecognizedField(fieldRow.Key);
            }
        }

        private void DrawUnrecognizedField(SerializedProperty field)
        {
            var name = field.FindPropertyRelative("fieldName").stringValue;
            var valueProperty = field.FindPropertyRelative("fieldValue");

            PropertyField propertyField = new PropertyField();
            propertyField.label = ObjectNames.NicifyVariableName(name);
            propertyField.BindProperty(valueProperty);
            propertyField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Debug.Log("Calling ApplyModifiedProperties");
                valueProperty.serializedObject.ApplyModifiedProperties();
            });
            unrecognizedFieldsFoldout.Add(propertyField);
        }
        private SerializedProperty GetOrCreateField(SerializedProperty collectionProperty, FieldInfo fieldInfo)
        {
            for (var i = 0; i < collectionProperty.arraySize; i++)
            {
                var field = collectionProperty.GetArrayElementAtIndex(i);
                if (field.FindPropertyRelative("fieldName").stringValue == fieldInfo.Name)
                {
                    return field;
                }
            }
            collectionProperty.arraySize++;

            var serializedField = collectionProperty.GetArrayElementAtIndex(collectionProperty.arraySize - 1);
            var fieldNameProperty = serializedField.FindPropertyRelative("fieldName");
            fieldNameProperty.stringValue = fieldInfo.Name;

            var fieldValueProperty = serializedField.FindPropertyRelative("fieldValue");

            (UnityEngine.Object objectReference, string serializedString) result = (null, null);
            if (_specialDefaultValueCreators.TryGetValue(fieldInfo.FieldType, out var creator))
            {
                SerializationMediator.SerializeFromFieldInfo(fieldInfo, creator(), out result);
            }
            else
            {
                SerializationMediator.SerializeFromFieldInfo(fieldInfo, fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : (object)null, out result);
            }

            fieldValueProperty.FindPropertyRelative("stringValue").stringValue = result.serializedString;
            fieldValueProperty.FindPropertyRelative("objectValue").objectReferenceValue = result.objectReference;

            boundProperty.serializedObject.ApplyModifiedProperties();
            return serializedField;
        }

        private VisualElement CreateControl(FieldInfo fieldInfo, SerializedProperty fieldProperty)
        {
            var tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();

            var serializedValueProperty = fieldProperty.FindPropertyRelative("fieldValue");
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
            {
                var objectValue = serializedValueProperty.FindPropertyRelative("objectValue");
                var objectField = new ObjectField
                {
                    label = ObjectNames.NicifyVariableName(fieldInfo.Name),
                    tooltip = tooltipAttribute?.tooltip ?? null,
                    objectType = fieldInfo.FieldType,
                };

                objectField.BindProperty(objectValue);
                return objectField;
            }
            else
            {
                var stringValue = serializedValueProperty.FindPropertyRelative("stringValue");
                var serializedValue = stringValue.stringValue;

                var ctrl = CreateStringControl(fieldInfo, fieldProperty, stringValue, serializedValue);
                ctrl.tooltip = tooltipAttribute?.tooltip ?? null;
                if(ctrl is PropertyField pf)
                {
                    pf.label = ObjectNames.NicifyVariableName(fieldInfo.Name);
                }
                return ctrl;
            }
        }

        private void OnAttached(AttachToPanelEvent evt)
        {
            clearUnrecognizedFieldsButton.clicked += ClearUnrecognizedFieldsButton_clicked;
        }

        private void ClearUnrecognizedFieldsButton_clicked()
        {
            foreach(var fieldRow in _unrecognizedFields.OrderByDescending(pair => pair.Value))
            {
                _serializedFieldsProperty.DeleteArrayElementAtIndex(fieldRow.Value);
            }
            _serializedFieldsProperty.serializedObject.ApplyModifiedProperties();
            _unrecognizedFields.Clear();
            unrecognizedFieldsFoldout.Clear();

            unrecognizedFieldContainer.SetDisplay(false);
            unrecognizedFieldContainer.SetEnabled(false);
        }

        public SerializedFieldCollectionElement()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(SerializedFieldCollectionElement), this);

            RegisterCallback<AttachToPanelEvent>(OnAttached);

            helpBox = new UnityEngine.UIElements.HelpBox("No Type is being serialized.", MessageType.Info, true, false);
            Add(helpBox);
            helpBox.SendToBack();

            container = this.Q<VisualElement>("MainContainer");
            staticFieldsFoldout = this.Q<Foldout>("StaticFields");
            instanceFieldsFoldout = this.Q<Foldout>("InstanceFields");

            unrecognizedFieldContainer = this.Q<VisualElement>("UnrecognizedFieldContainer");
            clearUnrecognizedFieldsButton = this.Q<Button>("ClearUnrecognizedFields");
            unrecognizedFieldsFoldout = this.Q<Foldout>("UnrecognizedFields");
        }

        #region Controller creation //Move to another class
        private VisualElement CreateStringControl(FieldInfo fieldInfo, SerializedProperty fieldProperty, SerializedProperty stringValue, string serializedValue)
        {
            var fieldType = fieldInfo.FieldType;

            if (!CanBuildControlFromType(fieldType))
            {
                var unrecognizedField = new PropertyField(fieldProperty);
                return unrecognizedField;
            }

            string nicifiedName = ObjectNames.NicifyVariableName(fieldInfo.Name);
            if (fieldType.IsEnum)
            {
                var flagsAttribute = fieldType.GetCustomAttribute<FlagsAttribute>();
                var element = flagsAttribute != null ? _enumFlagsControlBuilder(nicifiedName, stringValue, serializedValue, fieldInfo) : _enumControlBuilder(nicifiedName, stringValue, serializedValue, fieldInfo);

                return element;
            }

            return _typeToControlBuilder[fieldType](nicifiedName, stringValue, serializedValue, fieldInfo);
        }

        private static bool CanBuildControlFromFieldInfo(FieldInfo fInfo) => CanBuildControlFromType(fInfo.FieldType);
        private static bool CanBuildControlFromType(Type type)
        {
            return type.IsEnum || _typeToControlBuilder.ContainsKey(type);
        }

        private static Dictionary<Type, ControlBuilder> _typeToControlBuilder = new Dictionary<Type, ControlBuilder>();

        private static ControlBuilder _enumFlagsControlBuilder;
        private static ControlBuilder _enumControlBuilder;

        private delegate VisualElement ControlBuilder(string label, SerializedProperty stringValueProperty, string serializedValue, FieldInfo fieldInfo);

        static SerializedFieldCollectionElement()
        {
            Add<short>((l, sp, sv, fi) =>
            {
                return new Label($"Short Field is Not implemented yet");
            });
            Add<ushort>((l, sp, sv, fi) =>
            {
                return new Label($"UShort Field is Not implemented yet");
            });
            Add<int>((l, sp, sv, fi) =>
            {
                var field = new IntegerField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<uint>((l, sp, sv, fi) =>
            {
                return new Label($"UInt field is Not implemented yet");
            });
            Add<long>((l, sp, sv, fi) =>
            {
                var field = new LongField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<ulong>((l, sp, sv, fi) =>
            {
                return new Label($"ULong field is Not implemented yet");
            });
            Add<bool>((l, sp, sv, fi) =>
            {
                var field = new Toggle(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<float>((l, sp, sv, fi) =>
            {
                var field = new FloatField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<double>((l, sp, sv, fi) =>
            {
                var field = new DoubleField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<string>((l, sp, sv, fi) =>
            {
                var field = new TextField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Color>((l, sp, sv, fi) =>
            {
                var field = new ColorField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<LayerMask>((l, sp, sv, fi) =>
            {
                var field = new LayerMaskField(l);
                field.value = ((LayerMask)SerializationMediator.Deserialize(fi.FieldType, sv)).value;
                field.RegisterValueChangedCallback(evt =>
                {
                    SerializationMediator.SerializeFromFieldInfo(fi, evt.newValue, out var result);
                    sp.stringValue = result.serializedString;
                    sp.serializedObject.ApplyModifiedProperties();
                });
                return field;
            });
            Add<Vector2>((l, sp, sv, fi) =>
            {
                var field = new Vector2Field(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Vector2Int>((l, sp, sv, fi) =>
            {
                var field = new Vector2IntField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Vector3>((l, sp, sv, fi) =>
            {
                var field = new Vector3Field(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Vector3Int>((l, sp, sv, fi) =>
            {
                var field = new Vector3IntField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Vector4>((l, sp, sv, fi) =>
            {
                var field = new Vector4Field(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Rect>((l, sp, sv, fi) =>
            {
                var field = new RectField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<RectInt>((l, sp, sv, fi) =>
            {
                var field = new RectIntField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<char>((l, sp, sv, fi) =>
            {
                var field = new TextField(l, 1, false, false, '*');
                field.value = char.ToString((char)SerializationMediator.Deserialize(fi.FieldType, sv));
                field.RegisterValueChangedCallback(evt =>
                {
                    SerializationMediator.SerializeFromFieldInfo(fi, evt.newValue, out var result);
                    sp.stringValue = result.serializedString;
                    sp.serializedObject.ApplyModifiedProperties();
                });
                return field;
            });
            Add<Bounds>((l, sp, sv, fi) =>
            {
                var field = new BoundsField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<BoundsInt>((l, sp, sv, fi) =>
            {
                var field = new BoundsIntField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });
            Add<Quaternion>((l, sp, sv, fi) =>
            {
                var field = new Vector3Field(l);
                field.value = ((Quaternion)SerializationMediator.Deserialize(fi.FieldType, sv)).eulerAngles;
                field.RegisterValueChangedCallback(evt =>
                {
                    SerializationMediator.SerializeFromFieldInfo(fi, evt.newValue, out var result);
                    sp.stringValue = result.serializedString;
                    sp.serializedObject.ApplyModifiedProperties();
                });
                return field;
            });
            Add<AnimationCurve>((l, sp, sv, fi) =>
            {
                var field = new CurveField(l);
                SetupField(field, sp, sv, fi);
                return field;
            });

            _enumFlagsControlBuilder = (l, sp, sv, fi) =>
            {
                var enumFlagsField = new EnumFlagsField(l, (Enum)SerializationMediator.Deserialize(fi.FieldType, sv));
                SetupField(enumFlagsField, sp, sv, fi);
                return enumFlagsField;
            };
            _enumControlBuilder = (l, sp, sv, fi) =>
            {
                var enumField = new EnumField(l, (Enum)SerializationMediator.Deserialize(fi.FieldType, sv));
                SetupField(enumField, sp, sv, fi);
                return enumField;
            };

            void Add<T>(ControlBuilder func)
            {
                _typeToControlBuilder.Add(typeof(T), func);
            }
        }

        private static void SetupField<T>(BaseField<T> field, SerializedProperty stringProperty, string serializedValue, FieldInfo fieldInfo)
        {
            field.value = (T)SerializationMediator.Deserialize(fieldInfo.FieldType, serializedValue);
            field.RegisterValueChangedCallback(evt =>
            {
                SerializationMediator.SerializeFromFieldInfo(fieldInfo, evt.newValue, out var result);
                stringProperty.stringValue = result.serializedString;
                stringProperty.serializedObject.ApplyModifiedProperties();
            });
        }
        #endregion
    }
}