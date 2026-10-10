using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// A class containing a plethora of utility methods for creating and handling Visual Elements
    /// </summary>
    public static class VisualElementUtil
    {
        public struct ControlBuilderArgs
        {
            public string elementLabel;
            public Func<object> valueRetriever;
            public DeconstructedChangeEvent changeEvent;

            public FieldInfo fieldInfo;
            public SerializedProperty fieldProperty;
        }
        public delegate VisualElement ControlBuilder(ControlBuilderArgs args);

        private static Dictionary<Type, ControlBuilder> _typeToControlBuilder = new Dictionary<Type, ControlBuilder>();
        private static ControlBuilder _enumFlagsControlBuilder;
        private static ControlBuilder _enumIndexControlBuilder;

        /// <summary>
        /// Assigns <paramref name="controlBuilder"/> to the type in <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type that <paramref name="controlBuilder"/> builds a controller for</typeparam>
        /// <param name="controlBuilder">The controller builder</param>
        /// <returns>True if the controller builder was succesfully added, otherwise false.</returns>
        public static bool AddControlBuilderForType<T>(ControlBuilder controlBuilder) => AddControlBuilderForType(typeof(T), controlBuilder);

        /// <summary>
        /// Assigns <paramref name="controlBuilder"/> to the type in <paramref name="controlType"/>
        /// </summary>
        /// <param name="controlType">The type that <paramref name="controlBuilder"/> builds a controller for</param>
        /// <param name="controlBuilder">The controller builder</param>
        /// <returns>True if the controller builder was succesfully added, otherwise false.</returns>
        public static bool AddControlBuilderForType(Type controlType, ControlBuilder controlBuilder)
        {
            if(_typeToControlBuilder.ContainsKey(controlType))
            {
                RoR2EKLog.Error($"Cannot add control builder for type {controlType} as one already exists.");
                return false;
            }
            _typeToControlBuilder.Add(controlType, controlBuilder);
            return true;
        }

        /// <summary>
        /// Checks if its possible to build a Control for the specified type
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if a control can be created, otherwise false</returns>
        public static bool CanBuildControlForType(Type type)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(type) || type.IsEnum || _typeToControlBuilder.ContainsKey(type);
        }

        [Obsolete("Use CreateControlFromType<T>(ControlBuilderArgs) instead. Keep in mind the new method returns a VisualElement instead of a NotifyValueChanged<T>")]
        public static INotifyValueChanged<T> CreateControlFromType<T>(string label, Func<object> valueGetter, DeconstructedChangeEvent changeEvent) => (INotifyValueChanged<T>)CreateControlFromType(typeof(T), label, valueGetter, changeEvent);

        [Obsolete("Use CreateControlFromType<T>(ControlBuilderArgs) instead.")]
        public static VisualElement CreateControlFromType(Type type, string label, Func<object> valueGetter, DeconstructedChangeEvent changeEvent) => CreateControlFromType(type, new ControlBuilderArgs
        {
            changeEvent = changeEvent,
            elementLabel = label,
            valueRetriever = valueGetter
        });

        public static VisualElement CreateControlFromType<T>(ControlBuilderArgs args) => CreateControlFromType(typeof(T), args);

        public static VisualElement CreateControlFromType(Type type, ControlBuilderArgs args)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                var objectField = new ObjectField(args.elementLabel);
                objectField.objectType = type;
                objectField.value = (UnityEngine.Object)args.valueRetriever();
                objectField.RegisterValueChangedCallback(evt => args.changeEvent(new DeconstructedChangeEventData
                {
                    eventBase = evt,
                    newValue = evt.newValue,
                    previousValue = evt.previousValue
                }));
                return objectField;
            }

            if (type.IsEnum)
            {
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    return _enumFlagsControlBuilder(args);
                }
                return _enumIndexControlBuilder(args);
            }
            if (_typeToControlBuilder.TryGetValue(type, out var builder))
            {
                return builder(args);
            }

            return new Label($"Creation of control for type {type.Name} is not implemented.");
        }

        /// <summary>
        /// Directly sets <paramref name="objField"/>'s objectType using the provided generic
        /// </summary>
        /// <typeparam name="T">The type of object this object field accepts</typeparam>
        /// <param name="objField">The object field itself</param>
        public static void SetObjectType<T>(this ObjectField objField) where T : UnityEngine.Object
        {
            objField.objectType = typeof(T);
        }

        /// <summary>
        /// Quickly sets the display of a visual element
        /// </summary>
        /// <param name="visualElement">The element to change the display style</param>
        /// <param name="displayStyle">new display style value</param>
        public static void SetDisplay(this VisualElement visualElement, DisplayStyle displayStyle) => visualElement.style.display = displayStyle;

        /// <summary>
        /// Quickly sets the display of a visual elementt
        /// </summary>
        /// <param name="visualElement">The element to change the display style</param>
        /// <param name="display">True if its displayed, false if its hidden</param>
        public static void SetDisplay(this VisualElement visualElement, bool display) => visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;

        /// <summary>
        /// Normalizes a name for usage on UXML trait attributes
        /// <para>Due to limitations on UIBuilder, the UXML trait's name needs to have a specific name formtting that must match the required property that's going to set the value.</para>
        /// </summary>
        /// <param name="nameofProperty"></param>
        /// <returns>A normalized string for an UXML trait</returns>
        public static string NormalizeNameForUXMLTrait(string nameofProperty) => ObjectNames.NicifyVariableName(nameofProperty).ToLower().Replace(" ", "-");


        /// <summary>
        /// Represents a deconstructed version of a <see cref="ChangeEvent{T}"/>, since there's no non generic version of <see cref="ChangeEvent{T}"/>, this is used instead.
        /// </summary>
        /// <param name="deconstructedChangeEvent">The deconstructed change event</param>
        public delegate void DeconstructedChangeEvent(DeconstructedChangeEventData deconstructedChangeEvent);

        /// <summary>
        /// Represents a deconstructed form of <see cref="ChangeEvent{T}"/>
        /// </summary>
        public struct DeconstructedChangeEventData
        {
            /// <summary>
            /// The EventBase itself, this is technically a <see cref="ChangeEvent{T}"/> casted into an EventBase, so you can properly obtain data regarding how the change event was triggered.
            /// </summary>
            public EventBase eventBase;

            /// <summary>
            /// The new value, boxed into a System.Object.
            /// </summary>
            public object newValue;

            /// <summary>
            /// The previous value, boxed into a System.Object
            /// </summary>
            public object previousValue;
        }

        static VisualElementUtil()
        {
            AddControlBuilderForType<short>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new IntegerField(label);
                field.value = (short)valueGetter();
                field.RegisterValueChangedCallback(e =>
                {
                    int newValueAsInt = e.newValue;
                    if (newValueAsInt > short.MaxValue)
                    {
                        newValueAsInt = short.MaxValue;
                    }
                    else if (newValueAsInt < short.MinValue)
                    {
                        newValueAsInt = short.MinValue;
                    }
                    field.SetValueWithoutNotify(newValueAsInt);
                    short casted = Convert.ToInt16(newValueAsInt);

                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = e,
                        newValue = casted,
                        previousValue = (short)e.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<ushort>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new IntegerField(label);
                field.value = (ushort)valueGetter();
                field.RegisterValueChangedCallback(e =>
                {
                    int newValueAsInt = e.newValue;
                    if (newValueAsInt < 0)
                    {
                        newValueAsInt = 0;
                    }
                    else if (newValueAsInt > ushort.MaxValue)
                    {
                        newValueAsInt = ushort.MaxValue;
                    }
                    field.SetValueWithoutNotify(newValueAsInt);
                    ushort casted = Convert.ToUInt16(newValueAsInt);

                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = e,
                        newValue = casted,
                        previousValue = (ushort)e.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<int>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new IntegerField(label);
                field.value = (int)valueGetter();
                field.RegisterValueChangedCallback(e =>
                {
                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = e,
                        newValue = e.newValue,
                        previousValue = e.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<uint>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new LongField(label);
                field.value = (uint)valueGetter();
                field.RegisterValueChangedCallback(e =>
                {
                    long newValueAsLong = e.newValue;
                    if (newValueAsLong < 0)
                    {
                        newValueAsLong = 0;
                    }
                    else if (newValueAsLong > uint.MaxValue)
                    {
                        newValueAsLong = uint.MaxValue;
                    }

                    field.SetValueWithoutNotify(newValueAsLong);
                    uint casted = Convert.ToUInt32(newValueAsLong);

                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = e,
                        newValue = casted,
                        previousValue = (uint)e.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<long>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new LongField(label);
                field.value = (long)valueGetter();
                field.RegisterValueChangedCallback(e =>
                {
                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = e,
                        newValue = e.newValue,
                        previousValue = e.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<ulong>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new LongField(label);
                field.value = (long)(ulong)valueGetter();
                field.RegisterValueChangedCallback(e =>
                {
                    var valueAsLong = e.newValue;
                    if (valueAsLong < 0)
                    {
                        valueAsLong = 0;
                    }

                    field.SetValueWithoutNotify(valueAsLong);
                    var casted = (ulong)valueAsLong;

                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = e,
                        newValue = casted,
                        previousValue = (ulong)e.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<bool>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var toggle = new Toggle(label);
                GenericSetup(toggle, valueGetter, changeEvent);
                return toggle;
            });
            AddControlBuilderForType<float>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new FloatField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<double>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new DoubleField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<string>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new TextField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Color>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new ColorField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<LayerMask>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new LayerMaskField(label);
                field.value = ((LayerMask)valueGetter()).value;
                field.RegisterValueChangedCallback(evt =>
                {
                    changeEvent(new DeconstructedChangeEventData
                    {
                        eventBase = evt,
                        newValue = (LayerMask)evt.newValue,
                        previousValue = (LayerMask)evt.previousValue
                    });
                });
                return field;
            });
            AddControlBuilderForType<Vector2>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new Vector2Field(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Vector3>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new Vector3Field(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Vector4>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new Vector4Field(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Vector2Int>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new Vector2IntField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Vector3Int>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new Vector3IntField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Rect>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new RectField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<RectInt>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new RectIntField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<char>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new TextField(label, 1, false, false, '*');
                field.value = char.ToString((char)valueGetter());
                field.RegisterValueChangedCallback(evt => changeEvent(new DeconstructedChangeEventData
                {
                    eventBase = evt,
                    newValue = evt.newValue,
                    previousValue = evt.previousValue
                }));
                return field;
            });
            AddControlBuilderForType<Bounds>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new BoundsField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<BoundsInt>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new BoundsIntField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });
            AddControlBuilderForType<Quaternion>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new Vector3Field(label);
                field.value = ((Quaternion)valueGetter()).eulerAngles;
                field.RegisterValueChangedCallback(evt =>
                {
                    var newQuat = Quaternion.Euler(evt.newValue);
                    var previousQuat = Quaternion.Euler(evt.previousValue);

                    changeEvent(new DeconstructedChangeEventData
                    {
                        newValue = newQuat,
                        previousValue = previousQuat,
                        eventBase = evt
                    });
                });
                return field;
            });
            AddControlBuilderForType<AnimationCurve>((args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var field = new CurveField(label);
                GenericSetup(field, valueGetter, changeEvent);
                return field;
            });

            _enumFlagsControlBuilder = (args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var enumFlagsField = new EnumFlagsField(label, (Enum)valueGetter());
                GenericSetup(enumFlagsField, valueGetter, changeEvent);
                return enumFlagsField;
            };

            _enumIndexControlBuilder = (args) =>
            {
                var label = args.elementLabel;
                var valueGetter = args.valueRetriever;
                var changeEvent = args.changeEvent;

                var enumField = new EnumField(label, (Enum)valueGetter());
                GenericSetup(enumField, valueGetter, changeEvent);
                return enumField;
            };

            void GenericSetup<T>(BaseField<T> field, Func<object> getter, DeconstructedChangeEvent changeEvent)
            {
                field.value = (T)getter();
                field.RegisterValueChangedCallback(evt => changeEvent(new DeconstructedChangeEventData
                {
                    eventBase = evt,
                    newValue = evt.newValue,
                    previousValue = evt.previousValue
                }));
            }
        }
    }
}