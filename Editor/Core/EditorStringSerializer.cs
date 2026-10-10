using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// The EditorStringSerializer is a StringSerializer that's bundled with R2EK.
    /// <br></br>
    /// It has the same capabilities as the base game string serializer, alongside supporting all the string serialization capabilities of R2API_StringSerializerExtensions.
    /// <para></para>
    /// It is greatly recommended to utilize <see cref="SerializationMediator"/> instead, as the SerializationMediator will ensure only types that can be serialized in the current modding context can be. (IE: Not having the string serializer extensions installed will ensure you cannot serialize enums)
    /// </summary>
    public static class EditorStringSerializer
    {
        private static ReadOnlyCollection<Type> _serializableTypesReadOnly;
        private static readonly List<Type> _serializableTypes = new List<Type>();
        private static readonly Dictionary<Type, SerializationHandler> _typeToSerializationHandlers = new Dictionary<Type, SerializationHandler>();
        private static SerializationHandler _enumHandler;

        /// <summary>
        /// Returns a collection of the Serializable types
        /// </summary>
        public static ReadOnlyCollection<Type> GetSerializableTypes()
        {
            _serializableTypesReadOnly ??= new ReadOnlyCollection<Type>(_serializableTypes);
            return _serializableTypesReadOnly;
        }

        /// <summary>
        /// Returns true if <typeparamref name="T"/> can be serialized.
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        public static bool CanSerializeType<T>() => CanSerializeType(typeof(T));

        /// <summary>
        /// Returns true if <paramref name="t"/> can be serialized.
        /// </summary>
        /// <param name="t">The type to serialize</param>
        public static bool CanSerializeType(Type t) => t == typeof(Enum) || t.IsEnum || _typeToSerializationHandlers.ContainsKey(t);

        /// <summary>
        /// Serializes <paramref name="value"/> as a string. If no serialization handler exists, an empty string is returned.
        /// </summary>
        /// <typeparam name="T">The type of value to serialize</typeparam>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value, returns an empty string if no serialization handler exists</returns>
        public static string Serialize<T>(T value)
        {
            var type = typeof(T);
            return Serialize(type, value);
        }

        /// <summary>
        /// Serializes <paramref name="value"/> as a string. If no serialization handler exists, an empty string is returned.
        /// </summary>
        /// <param name="type">The type of value to serialize</param>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value, returns an empty string if no serialization handler exists</returns>
        public static string Serialize(Type type, object value)
        {
            if (type.IsEnum)
            {
                return _enumHandler.serializer(value);
            }
            if (_typeToSerializationHandlers.TryGetValue(type, out var handler))
            {
                return handler.serializer(value);
            }
            return string.Empty;
        }

        /// <summary>
        /// Deserializes the <paramref name="input"/> into an instance of <typeparamref name="T"/>. If no serialization handler exists, the default object is returned.
        /// </summary>
        /// <typeparam name="T">The type of value to deserialize</typeparam>
        /// <param name="input">The value to deserialize</param>
        /// <returns>The deserialized value, or a default object if no serialization handler exists.</returns>
        public static T Deserialize<T>(string input)
        {
            var type = typeof(T);
            return (T)Deserialize(type, input);
        }

        /// <summary>
        /// Deserializes the <paramref name="input"/> into an instance of <paramref name="type"/>. If no serialization handler exists, the default object is returned.
        /// </summary>
        /// <param name="type">The type of value to deserialize</param>
        /// <param name="input">The value to deserialize</param>
        /// <returns>The deserialized value, or a default object if no serialization handler exists</returns>
        public static object Deserialize(Type type, string input)
        {
            if (type.IsEnum)
            {
                return _enumHandler.deserializer(input);
            }
            if (_typeToSerializationHandlers.TryGetValue(type, out var handler))
            {
                return handler.deserializer(input);
            }
            return default;
        }

        /// <summary>
        /// Adds a SerializationHandler to the EditorStringSerializer
        /// </summary>
        /// <typeparam name="T">The Type that the serialization handler serializes</typeparam>
        /// <param name="serializationHandler">The serialization handler</param>
        public static void AddSerializationHandler<T>(DeserializationDelegate deserializationDelegate, SerializationDelegate serializationDelegate) => AddSerializationHandler(typeof(T), deserializationDelegate, serializationDelegate);

        /// <summary>
        /// Adds a SerializationHandler to the EditorStringSerializer
        /// </summary>
        /// <param name="type">The Type that the serialization handler serializes</param>
        /// <param name="deserialization">The deserialization method, see <see cref="DeserializationDelegate"/></param>
        /// <param name="serialization">The serialization method, see <see cref="SerializationDelegate"/></param>
        public static void AddSerializationHandler(Type type, DeserializationDelegate deserialization, SerializationDelegate serialization) => AddSerializationHandlerInternal(type, new SerializationHandler
        {
            serializer = serialization,
            deserializer = deserialization,
        });

        private static void AddSerializationHandlerInternal<T>(SerializationHandler handler) => AddSerializationHandlerInternal(typeof(T), handler);
        private static void AddSerializationHandlerInternal(Type type, SerializationHandler serializationHandler)
        {
            if (_typeToSerializationHandlers.ContainsKey(type))
            {
                RoR2EKLog.Error($"Cannot add serialization delegate for type {type.FullName} as a Serializer already exists.");
                return;
            }

            _serializableTypes.Add(type);
            _typeToSerializationHandlers.Add(type, serializationHandler);
        }

        /// <summary>
        /// Tries to split the <paramref name="input"/> using ','. If the result of the split is less than <paramref name="minComponentCount"/>, a warning is returned an an empty array is assigned to <paramref name="output"/>
        /// </summary>
        /// <typeparam name="T">The type that <paramref name="input"/> is supposed to represent</typeparam>
        /// <param name="input">The string input</param>
        /// <param name="minComponentCount">The minimum amount of components the split string should produce</param>
        /// <param name="output">The output string array, if the split output is less than <paramref name="minComponentCount"/>, it's an empty array</param>
        /// <returns>True if the splitting procedure is succesful</returns>
        public static bool TrySplit<T>(string input, int minComponentCount, out string[] output)
        {
            output = input.Split(',');
            if (output.Length < minComponentCount)
            {
                RoR2EKLog.Warning($"Too few elements ({output.Length}/{minComponentCount}) for {typeof(T).FullName}");
                output = Array.Empty<string>();
                return false;
            }
            return true;
        }

        static EditorStringSerializer()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            AddSerializationHandlerInternal<short>(new SerializationHandler
            {
                deserializer = (txt) => short.Parse(txt, culture),
                serializer = (obj) => ((short)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<ushort>(new SerializationHandler
            {
                deserializer = (txt) => ushort.Parse(txt, culture),
                serializer = (obj) => ((ushort)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<int>(new SerializationHandler
            {
                deserializer = (txt) => int.Parse(txt, culture),
                serializer = (obj) => ((int)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<uint>(new SerializationHandler
            {
                deserializer = txt => uint.Parse(txt, culture),
                serializer = (obj) => ((uint)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<long>(new SerializationHandler
            {
                deserializer = (str) => long.Parse(str, culture),
                serializer = (obj) => ((long)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<ulong>(new SerializationHandler
            {
                deserializer = txt => ulong.Parse(txt),
                serializer = obj => ((ulong)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<bool>(new SerializationHandler
            {
                deserializer = txt => int.Parse(txt, culture) > 0,
                serializer = obj => ((bool)obj) ? "1" : "0"
            });
            AddSerializationHandlerInternal<float>(new SerializationHandler
            {
                deserializer = txt => float.Parse(txt, culture),
                serializer = obj => ((float)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<double>(new SerializationHandler
            {
                deserializer = txt => double.Parse(txt, culture),
                serializer = obj => ((double)obj).ToString(culture)
            });
            AddSerializationHandlerInternal<string>(new SerializationHandler
            {
                deserializer = txt => txt,
                serializer = obj => (string)obj
            });
            AddSerializationHandlerInternal<Color>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Color>(txt, 4, out var output))
                    {
                        return new Color
                        {
                            r = float.Parse(output[0], culture),
                            g = float.Parse(output[1], culture),
                            b = float.Parse(output[2], culture),
                            a = float.Parse(output[3], culture)
                        };
                    }
                    return Color.white;
                },
                serializer = obj =>
                {
                    var asColor = (Color)obj;
                    return $"{asColor.r.ToString(culture)}, {asColor.g.ToString(culture)}, {asColor.b.ToString(culture)}, {asColor.a.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<LayerMask>(new SerializationHandler
            {
                deserializer = txt => new LayerMask { value = int.Parse(txt, culture) },
                serializer = obj =>
                {
                    if (obj is int @int)
                    {
                        return @int.ToString(culture);
                    }
                    return ((LayerMask)obj).value.ToString(culture);
                }
            });
            AddSerializationHandlerInternal<Vector2>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Vector2>(txt, 2, out var output))
                    {
                        return new Vector2
                        {
                            x = float.Parse(output[0], culture),
                            y = float.Parse(output[1], culture)
                        };
                    }
                    return Vector2.zero;
                },
                serializer = obj =>
                {
                    var vector2 = (Vector2)obj;
                    return $"{vector2.x.ToString(culture)}, {vector2.y.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<Vector2Int>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Vector2Int>(txt, 2, out var output))
                    {
                        return new Vector2Int
                        {
                            x = int.Parse(output[0], culture),
                            y = int.Parse(output[1], culture)
                        };
                    }
                    return Vector2Int.zero;
                },
                serializer = obj =>
                {
                    var vector2 = (Vector2Int)obj;
                    return $"{vector2.x.ToString(culture)}, {vector2.y.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<Vector3>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Vector3>(txt, 3, out var output))
                    {
                        return new Vector3
                        {
                            x = float.Parse(output[0], culture),
                            y = float.Parse(output[1], culture),
                            z = float.Parse(output[2], culture)
                        };
                    }
                    return Vector3.zero;
                },
                serializer = obj =>
                {
                    var vector3 = (Vector3)obj;
                    return $"{vector3.x.ToString(culture)}, {vector3.y.ToString(culture)}, {vector3.z.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<Vector3Int>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Vector3Int>(txt, 3, out var output))
                    {
                        return new Vector3Int
                        {
                            x = int.Parse(output[0], culture),
                            y = int.Parse(output[1], culture),
                            z = int.Parse(output[2], culture)
                        };
                    }
                    return Vector3Int.zero;
                },
                serializer = obj =>
                {
                    var vector3 = (Vector3Int)obj;
                    return $"{vector3.x.ToString(culture)}, {vector3.y.ToString(culture)}, {vector3.z.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<Vector4>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Vector4>(txt, 4, out var output))
                    {
                        return new Vector4
                        {
                            x = float.Parse(output[0], culture),
                            y = float.Parse(output[1], culture),
                            z = float.Parse(output[2], culture),
                            w = float.Parse(output[3], culture)
                        };
                    }
                    return Vector4.zero;
                },
                serializer = obj =>
                {
                    var vector4 = (Vector4)obj;
                    return $"{vector4.x.ToString(culture)}, {vector4.y.ToString(culture)}, {vector4.z.ToString(culture)}, {vector4.z.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<Rect>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Rect>(txt, 4, out var output))
                    {
                        return new Rect
                        {
                            x = float.Parse(output[0], culture),
                            y = float.Parse(output[1], culture),
                            width = float.Parse(output[2], culture),
                            height = float.Parse(output[3], culture)
                        };
                    }
                    return Rect.zero;
                },
                serializer = obj =>
                {
                    var rect = (Rect)obj;
                    return $"{rect.x.ToString(culture)}, {rect.y.ToString(culture)}, {rect.width.ToString(culture)}, {rect.height.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<RectInt>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<RectInt>(txt, 4, out var output))
                    {
                        return new RectInt
                        {
                            x = int.Parse(output[0], culture),
                            y = int.Parse(output[1], culture),
                            width = int.Parse(output[2], culture),
                            height = int.Parse(output[3], culture)
                        };
                    }
                    return new RectInt(0, 0, 0, 0);
                },
                serializer = obj =>
                {
                    var rect = (RectInt)obj;
                    return $"{rect.x.ToString(culture)}, {rect.y.ToString(culture)}, {rect.width.ToString(culture)}, {rect.height.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<char>(new SerializationHandler
            {
                deserializer = txt => txt.ToCharArray().FirstOrDefault(),
                serializer = obj =>
                {
                    if (obj is string @string)
                    {
                        return @string.ToString(culture);
                    }
                    return ((char)obj).ToString(culture);
                }
            });
            AddSerializationHandlerInternal<Bounds>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Bounds>(txt, 6, out var output))
                    {
                        var center = new Vector3
                        {
                            x = float.Parse(output[0], culture),
                            y = float.Parse(output[1], culture),
                            z = float.Parse(output[2], culture)
                        };
                        var size = new Vector3
                        {
                            x = float.Parse(output[3], culture),
                            y = float.Parse(output[4], culture),
                            z = float.Parse(output[5], culture)
                        };
                        return new Bounds(center, size);
                    }
                    return new Bounds(Vector3.zero, Vector3.zero);
                },
                serializer = obj =>
                {
                    var bounds = (Bounds)obj;
                    var center = bounds.center;
                    var size = bounds.size;
                    return $"{center.x.ToString(culture)}, {center.y.ToString(culture)}, {center.z.ToString(culture)}, {size.x.ToString(culture)}, {size.y.ToString(culture)}, {size.z.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<BoundsInt>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<BoundsInt>(txt, 6, out var output))
                    {
                        var center = new Vector3Int
                        {
                            x = int.Parse(output[0], culture),
                            y = int.Parse(output[1], culture),
                            z = int.Parse(output[2], culture)
                        };
                        var size = new Vector3Int
                        {
                            x = int.Parse(output[3], culture),
                            y = int.Parse(output[4], culture),
                            z = int.Parse(output[5], culture)
                        };
                        return new BoundsInt(center, size);
                    }
                    return new BoundsInt(Vector3Int.zero, Vector3Int.zero);
                },
                serializer = obj =>
                {
                    var bounds = (BoundsInt)obj;
                    var center = Vector3Int.RoundToInt(bounds.center);
                    var size = Vector3Int.RoundToInt(bounds.size);
                    return $"{center.x.ToString(culture)}, {center.y.ToString(culture)}, {center.z.ToString(culture)}, {size.x.ToString(culture)}, {size.y.ToString(culture)}, {size.z.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<Quaternion>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (TrySplit<Quaternion>(txt, 4, out var output))
                    {
                        Quaternion quat = new Quaternion
                        {
                            x = float.Parse(output[0], culture),
                            y = float.Parse(output[1], culture),
                            z = float.Parse(output[2], culture),
                            w = float.Parse(output[3], culture),
                        };
                        return quat;
                    }
                    return Quaternion.identity;
                },
                serializer = obj =>
                {
                    Quaternion quat = (Quaternion)obj;
                    return $"{quat.x.ToString(culture)}, {quat.y.ToString(culture)}, {quat.z.ToString(culture)}, {quat.w.ToString(culture)}";
                }
            });
            AddSerializationHandlerInternal<AnimationCurve>(new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (string.IsNullOrEmpty(txt))
                        return new object();
                    AnimationCurveJSONIntermediate intermediate = JsonUtility.FromJson<AnimationCurveJSONIntermediate>(txt);
                    return AnimationCurveJSONIntermediate.ToAnimationCurve(intermediate);
                },
                serializer = obj =>
                {
                    var animationCurve = (AnimationCurve)obj;
                    return JsonUtility.ToJson(AnimationCurveJSONIntermediate.FromAnimationCurve(animationCurve));
                }
            });
            _enumHandler = new SerializationHandler
            {
                deserializer = txt =>
                {
                    if (string.IsNullOrEmpty(txt))
                        return new object();

                    EnumJSONIntermediate intermediate = JsonUtility.FromJson<EnumJSONIntermediate>(txt);
                    return EnumJSONIntermediate.ToEnum(intermediate);
                },
                serializer = obj =>
                {
                    var @enum = (Enum)obj;
                    return JsonUtility.ToJson(EnumJSONIntermediate.FromEnum(@enum));
                }
            };
        }

        /// <summary>
        /// Represents a delegate used to Deserialize <paramref name="serializedValue"/> into an <see cref="object"/>
        /// </summary>
        /// <param name="serializedValue">The string representation of the object</param>
        /// <returns>The deserialized object</returns>
        public delegate object DeserializationDelegate(string serializedValue);

        /// <summary>
        /// Represents a delegate used to Serialize <paramref name="valueToSerialize"/> into a <see cref="string"/>
        /// </summary>
        /// <param name="valueToSerialize">The object to serialize as a string</param>
        /// <returns>The serialized string representation</returns>
        public delegate string SerializationDelegate(object valueToSerialize);

        internal struct SerializationHandler
        {
            public DeserializationDelegate deserializer;
            public SerializationDelegate serializer;
        }

        [Serializable]
        private struct AnimationCurveJSONIntermediate
        {
            public WrapMode preWrapMode;
            public WrapMode postWrapMode;

            public KeyFrameJSONIntermediate[] keys;

            public static AnimationCurve ToAnimationCurve(AnimationCurveJSONIntermediate intermediate)
            {
                KeyFrameJSONIntermediate[] array = intermediate.keys ?? Array.Empty<KeyFrameJSONIntermediate>();
                Keyframe[] array2 = new Keyframe[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array2[i] = KeyFrameJSONIntermediate.ToKeyframe(in array[i]);
                }
                return new AnimationCurve
                {
                    preWrapMode = intermediate.preWrapMode,
                    postWrapMode = intermediate.postWrapMode,
                    keys = array2
                };
            }

            public static AnimationCurveJSONIntermediate FromAnimationCurve(AnimationCurve src)
            {
                Keyframe[] array = src.keys;
                KeyFrameJSONIntermediate[] array2 = new KeyFrameJSONIntermediate[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array2[i] = KeyFrameJSONIntermediate.FromKeyframe(array[i]);
                }
                AnimationCurveJSONIntermediate result = default(AnimationCurveJSONIntermediate);
                result.preWrapMode = src.preWrapMode;
                result.postWrapMode = src.postWrapMode;
                result.keys = array2;
                return result;
            }
        }

        [Serializable]
        private struct KeyFrameJSONIntermediate
        {
            public float time;

            public float value;

            public float inTangent;

            public float outTangent;

            public float inWeight;

            public float outWeight;

            public WeightedMode weightedMode;

            public int tangentMode;

            public static Keyframe ToKeyframe(in KeyFrameJSONIntermediate intermediate)
            {
                Keyframe result = default(Keyframe);
                result.time = intermediate.time;
                result.value = intermediate.value;
                result.inTangent = intermediate.inTangent;
                result.outTangent = intermediate.outTangent;
                result.inWeight = intermediate.inWeight;
                result.outWeight = intermediate.outWeight;
                result.weightedMode = intermediate.weightedMode;
#pragma warning disable CS0618 // Type or member is obsolete
                result.tangentMode = intermediate.tangentMode;
#pragma warning restore CS0618 // Type or member is obsolete
                return result;
            }

            public static KeyFrameJSONIntermediate FromKeyframe(Keyframe src)
            {
                KeyFrameJSONIntermediate result = default(KeyFrameJSONIntermediate);
                result.time = src.time;
                result.value = src.value;
                result.inTangent = src.inTangent;
                result.outTangent = src.outTangent;
                result.inWeight = src.inWeight;
                result.outWeight = src.outWeight;
                result.weightedMode = src.weightedMode;
#pragma warning disable CS0618 // Type or member is obsolete
                result.tangentMode = src.tangentMode;
#pragma warning restore CS0618 // Type or member is obsolete
                return result;
            }
        }

        [Serializable]
        private struct EnumJSONIntermediate
        {
            public string assemblyQualifiedName;
            public string values;

            public static Enum ToEnum(in EnumJSONIntermediate intermediate)
            {
                return (Enum)Enum.Parse(Type.GetType(intermediate.assemblyQualifiedName), intermediate.values);
            }

            public static EnumJSONIntermediate FromEnum(Enum src)
            {
                return new EnumJSONIntermediate
                {
                    assemblyQualifiedName = src.GetType().AssemblyQualifiedName,
                    values = src.ToString()
                };
            }
        }
    }
}