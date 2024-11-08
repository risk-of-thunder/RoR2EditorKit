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
    /// <summary>
    /// The <see cref="SerializedFieldCollectionElement"/> is a VisualElement that allows you to easily display the serialized fields that are stored within a HG.GeneralSerializer.SerializedfieldCollection. this is used within the inspector for the game's EntityStateConfiguration system.
    /// 
    /// <para>Theoretically, you can use this element to create custom serialization setups for your own mods.</para>
    /// </summary>
    public class SerializedFieldCollectionElement : VisualElement
    {
        private static readonly Dictionary<Type, Func<object>> _specialDefaultValueCreators = new Dictionary<Type, Func<object>>
        {
            [typeof(AnimationCurve)] = () => new AnimationCurve(),
            [typeof(Gradient)] = () => new Gradient(),
        };

        /// <summary>
        /// The type we're currently serializing, changing this value will refresh the collection
        /// </summary>
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

        /// <summary>
        /// The help box for this visual element, which displays useful information
        /// </summary>
        public ExtendedHelpBox helpBox { get; private set; }

        /// <summary>
        /// The container for the visual element
        /// </summary>
        public VisualElement container { get; private set; }

        /// <summary>
        /// A foldout which contains all the controls for static fields found within <see cref="typeBeingSerialized"/>
        /// </summary>
        public Foldout staticFieldsFoldout { get; private set; }

        /// <summary>
        /// A Foldout which contains all the controls for static fields found within <see cref="typeBeingSerialized"/>
        /// </summary>
        public Foldout instanceFieldsFoldout { get; private set; }

        /// <summary>
        /// A container which contains all the controls for entries that are unrecognized for the <see cref="typeBeingSerialized"/>
        /// </summary>
        public VisualElement unrecognizedFieldContainer { get; private set; }

        /// <summary>
        /// A button which can be used to clear all unrecognized fields
        /// </summary>
        public Button clearUnrecognizedFieldsButton { get; private set; }
        public Foldout unrecognizedFieldsFoldout { get; private set; }

        /// <summary>
        /// The bound property for this collection element, this MUST be a SerializedProperty that represents a "HG.GeneralSerializer.SerializedFieldCollection"
        /// </summary>
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

        /// <summary>
        /// Checks for the type being serialized and updates the UI accordingly
        /// </summary>
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
                bool doesNotHaveAttribute = !fInfo.GetCustomAttributes<NonSerializedAttribute>().Any();
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
                if (ctrl is PropertyField pf)
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
            foreach (var fieldRow in _unrecognizedFields.OrderByDescending(pair => pair.Value))
            {
                _serializedFieldsProperty.DeleteArrayElementAtIndex(fieldRow.Value);
            }
            _serializedFieldsProperty.serializedObject.ApplyModifiedProperties();
            _unrecognizedFields.Clear();
            unrecognizedFieldsFoldout.Clear();

            unrecognizedFieldContainer.SetDisplay(false);
            unrecognizedFieldContainer.SetEnabled(false);
        }

        private VisualElement CreateStringControl(FieldInfo fieldInfo, SerializedProperty fieldProperty, SerializedProperty stringValue, string serializedValue)
        {
            var fieldType = fieldInfo.FieldType;

            if (!VisualElementUtil.CanBuildControlForType(fieldType))
            {
                var unrecognizedField = new PropertyField(fieldProperty);
                return unrecognizedField;
            }

            string label = ObjectNames.NicifyVariableName(fieldInfo.Name);
            Func<object> valueGetter = () => SerializationMediator.Deserialize(fieldType, serializedValue);
            VisualElementUtil.DeconstructedChangeEvent changeEvent = data =>
            {
                SerializationMediator.SerializeFromFieldInfo(fieldInfo, data.newValue, out var result);
                stringValue.stringValue = result.serializedString;
                stringValue.serializedObject.ApplyModifiedProperties();
            };
            return VisualElementUtil.CreateControlFromType(fieldType, label, valueGetter, changeEvent);
        }

        /// <summary>
        /// Constructor for a new <see cref="SerializedFieldCollectionElement"/> instance
        /// </summary>
        public SerializedFieldCollectionElement()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(SerializedFieldCollectionElement), this);

            RegisterCallback<AttachToPanelEvent>(OnAttached);

            helpBox = new ExtendedHelpBox("No Type is being serialized.", MessageType.Info, true, false);
            Add(helpBox);
            helpBox.SendToBack();

            container = this.Q<VisualElement>("MainContainer");
            staticFieldsFoldout = this.Q<Foldout>("StaticFields");
            instanceFieldsFoldout = this.Q<Foldout>("InstanceFields");

            unrecognizedFieldContainer = this.Q<VisualElement>("UnrecognizedFieldContainer");
            clearUnrecognizedFieldsButton = this.Q<Button>("ClearUnrecognizedFields");
            unrecognizedFieldsFoldout = this.Q<Foldout>("UnrecognizedFields");
        }
    }
}
