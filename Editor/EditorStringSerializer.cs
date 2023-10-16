using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HG.GeneralSerializer;
using UnityEngine;

namespace RoR2EditorKit
{
    /// <summary>
    /// An extension of the <see cref="HG.GeneralSerializer.StringSerializer"/>
    /// <br>Note that this should not be used on anything but editor related scripts, as it contains extra serializers.</br>
    /// <br>The EditorStringSerializer calls first the <see cref="HG.GeneralSerializer.StringSerializer"/> and checks if RoR2 already supports it, if not, RoR2EK uses custom handlers for serializing extra values.</br>
    /// <para>It includes extra serializers that are not found in the HG.GeneralSerializer, which includes:</para>
    /// <list type="bullet">
    /// <item>LayerMask</item>
    /// <item>Vector4</item>
    /// <item>Rect</item>
    /// <item>RectInt</item>
    /// <item>char</item>
    /// <item>Bounds</item>
    /// <item>BoundsInt</item>
    /// <item>Quaternion</item>
    /// <item>Vector2Int</item>
    /// <item>Vector3Int</item>
    /// <item>Any Enum</item>
    /// </list>
    /// <br>If you feel like there should be another special serializer, create an issue in the Github</br>
    /// </summary>
    public static class EditorStringSerializer
    {
        private static readonly Dictionary<Type, SerializationHandler> _extendedSerializationHandlers = new Dictionary<Type, SerializationHandler>();
        private static readonly SerializationHandler _enumHandler;
        private static CultureInfo Invariant => CultureInfo.InvariantCulture;
        public static string[] SplitToComponents(string str, Type type, int minimumComponentCount)
        {
            string[] array = str.Split(' ');
            if (array.Length < minimumComponentCount)
            {
                throw new FormatException($"Too few elements ({array.Length}/{minimumComponentCount}) for {type.FullName}");
            }
            return array;
        }

        public static bool CanSerializeType<T>() => CanSerializeType(typeof(T));
        public static bool CanSerializeType(Type type)
        {
            var canSerializeByDefault = StringSerializer.CanSerializeType(type);
            if(canSerializeByDefault)
            {
                return true;
            }
            return _extendedSerializationHandlers.ContainsKey(type) || type.IsEnum;
        }

        public static T Deserialize<T>(string str)
        {
            return (T)Deserialize(str, typeof(T));
        }
        public static object Deserialize(string str, Type type)
        {
            object value = StringSerializer.Deserialize(type, str);
            if (value != null)
                return value;

            if (type.IsEnum)
            {
                return DeserializeInternal(str, _enumHandler);
            }

            if (_extendedSerializationHandlers.TryGetValue(type, out var handler))
            {
                return DeserializeInternal(str, handler);
            }
            return null;

            object DeserializeInternal(string val, SerializationHandler hndlr)
            {
                try
                {
                    return hndlr.deserializer(val);
                }
                catch (Exception exception)
                {
                    throw new StringSerializerException(null, exception);
                }
            }
        }
        public static string Serialize<T>(T value)
        {
            return Serialize(value, typeof(T));
        }
        public static string Serialize(object value, Type type)
        {
            string serializedValue = StringSerializer.Serialize(type, value);
            if (!serializedValue.IsNullOrEmptyOrWhitespace())
            {
                return serializedValue;
            }

            if (type.IsEnum)
            {
                return SerializeInternal(value, _enumHandler);
            }

            if (_extendedSerializationHandlers.TryGetValue(type, out var handler))
            {
                return SerializeInternal(value, handler);
            }
            return null;

            string SerializeInternal(object val, SerializationHandler hndlr)
            {
                try
                {
                    return hndlr.serializer(val);
                }
                catch (Exception exception)
                {
                    throw new StringSerializerException(null, exception);
                }
            }
        }

        static EditorStringSerializer()
        {

            _extendedSerializationHandlers.Add(typeof(LayerMask), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    LayerMask mask = (LayerMask)obj;
                    return mask.value.ToString(Invariant);
                },
                deserializer = (str) =>
                {
                    LayerMask mask = new LayerMask { value = int.Parse(str, Invariant) };
                    return mask;
                }
            });

            _extendedSerializationHandlers.Add(typeof(Vector4), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    Vector4 vector = (Vector4)obj;
                    return $"{vector.x.ToString(Invariant)} {vector.y.ToString(Invariant)} {vector.z.ToString(Invariant)} {vector.w.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(Vector4), 4);
                    return new Vector4(float.Parse(components[0], Invariant), float.Parse(components[1], Invariant), float.Parse(components[2], Invariant), float.Parse(components[3], Invariant));
                }
            });

            _extendedSerializationHandlers.Add(typeof(Rect), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    Rect rect = (Rect)obj;
                    return $"{rect.x.ToString(Invariant)} {rect.y.ToString(Invariant)} {rect.width.ToString(Invariant)} {rect.height.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(Rect), 4);
                    return new Rect(float.Parse(components[0], Invariant), float.Parse(components[1], Invariant), float.Parse(components[2], Invariant), float.Parse(components[3], Invariant));
                }
            });

            _extendedSerializationHandlers.Add(typeof(RectInt), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    RectInt rect = (RectInt)obj;
                    return $"{rect.x.ToString(Invariant)} {rect.y.ToString(Invariant)} {rect.width.ToString(Invariant)} {rect.height.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(RectInt), 4);
                    return new RectInt(int.Parse(components[0], Invariant), int.Parse(components[1], Invariant), int.Parse(components[2], Invariant), int.Parse(components[3], Invariant));
                }
            });

            _extendedSerializationHandlers.Add(typeof(char), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    char character = (char)obj;
                    return character.ToString(Invariant);
                },
                deserializer = (str) =>
                {
                    return char.Parse(str);
                }
            });

            _extendedSerializationHandlers.Add(typeof(Bounds), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    Bounds bounds = (Bounds)obj;
                    return $"{bounds.center.x.ToString(Invariant)} {bounds.center.y.ToString(Invariant)} {bounds.center.z.ToString(Invariant)} " +
                    $"{bounds.size.x.ToString(Invariant)} {bounds.size.y.ToString(Invariant)} {bounds.size.z.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(Bounds), 6);
                    Vector3 center = new Vector3(float.Parse(components[0], Invariant), float.Parse(components[1], Invariant), float.Parse(components[2], Invariant));
                    Vector3 size = new Vector3(float.Parse(components[3], Invariant), float.Parse(components[4], Invariant), float.Parse(components[5], Invariant));
                    return new Bounds(center, size);
                }
            });

            _extendedSerializationHandlers.Add(typeof(BoundsInt), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    BoundsInt bounds = (BoundsInt)obj;
                    return $"{bounds.position.x.ToString(Invariant)} {bounds.position.y.ToString(Invariant)} {bounds.position.z.ToString(Invariant)} " +
                    $"{bounds.size.x.ToString(Invariant)} {bounds.size.y.ToString(Invariant)} {bounds.size.z.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(BoundsInt), 6);
                    Vector3Int position = new Vector3Int(int.Parse(components[0], Invariant), int.Parse(components[1], Invariant), int.Parse(components[2], Invariant));
                    Vector3Int size = new Vector3Int(int.Parse(components[3], Invariant), int.Parse(components[4], Invariant), int.Parse(components[5], Invariant));
                    return new BoundsInt
                    {
                        position = position,
                        size = size,
                    };
                }
            });

            _extendedSerializationHandlers.Add(typeof(Quaternion), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    Quaternion quat = (Quaternion)obj;
                    return $"{quat.x.ToString(Invariant)} {quat.y.ToString(Invariant)} {quat.z.ToString(Invariant)} {quat.w.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(Quaternion), 4);
                    return new Quaternion(float.Parse(components[0], Invariant), float.Parse(components[1], Invariant), float.Parse(components[2], Invariant), float.Parse(components[3], Invariant));
                }
            });

            _extendedSerializationHandlers.Add(typeof(Vector2Int), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    Vector2Int vector = (Vector2Int)obj;
                    return $"{vector.x.ToString(Invariant)} {vector.y.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(Vector2Int), 2);
                    return new Vector2Int(int.Parse(components[0], Invariant), int.Parse(components[1], Invariant));
                }
            });

            _extendedSerializationHandlers.Add(typeof(Vector3Int), new SerializationHandler
            {
                serializer = (obj) =>
                {
                    Vector3Int vector = (Vector3Int)obj;
                    return $"{vector.x.ToString(Invariant)} {vector.y.ToString(Invariant)} {vector.z.ToString(Invariant)}";
                },
                deserializer = (str) =>
                {
                    string[] components = SplitToComponents(str, typeof(Vector3Int), 3);
                    return new Vector3Int(int.Parse(components[0], Invariant), int.Parse(components[1], Invariant), int.Parse(components[2], Invariant));
                }
            });

            _enumHandler = new SerializationHandler
            {
                serializer = (obj) => JsonUtility.ToJson(EnumJSONIntermediate.ToJSON((Enum)obj)),
                deserializer = (str) =>
                {
                    if (string.IsNullOrEmpty(str))
                        return default(Enum);
                    EnumJSONIntermediate intermediate = JsonUtility.FromJson<EnumJSONIntermediate>(str);
                    return EnumJSONIntermediate.ToEnum(in intermediate);
                }
            };
        }
        public delegate string SerializerDelegate(object obj);
        public delegate object DeserializerDelegate(string str);

        private struct EnumJSONIntermediate
        {
            public string assemblyQualifiedName;
            public string values;

            public static Enum ToEnum(in EnumJSONIntermediate intermediate)
            {
                return (Enum)Enum.Parse(Type.GetType(intermediate.assemblyQualifiedName), intermediate.values);
            }

            public static EnumJSONIntermediate ToJSON(Enum @enum)
            {
                EnumJSONIntermediate result = new EnumJSONIntermediate
                {
                    assemblyQualifiedName = @enum.GetType().AssemblyQualifiedName,
                    values = @enum.ToString()
                };
                return result;
            }
        }

        private struct SerializationHandler
        {
            public SerializerDelegate serializer;

            public DeserializerDelegate deserializer;
        }
    }
}
