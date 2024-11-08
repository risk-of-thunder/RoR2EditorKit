using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="SerializationMediator"/> is a class that mediates the String serialization capabilities built into RoR2EditorKit. it's used for managing what fields can be serialized by the <see cref="SerializedFieldCollectionElement"/>. and the methods for creating control elements within <see cref="VisualElementUtil"/> 
    /// <br>You can bypass this mediator by using <see cref="SafetyBypass"/>'s methods</br>
    /// 
    /// <para>The types that can be serialized change depending on what assemblies are found within the project.</para>
    /// <list type="bullet">
    ///     <item>
    ///         If RoR2 is not installed, all the serialization capabilities are enabled. for a comprehensive list you can see <see cref="SafetyBypass"/>'s documentation.
    ///     </item>
    ///     <item>
    ///         If RoR2 IS installed, only the base game's serialization handlers are enabled.
    ///     </item>
    ///     <item>
    ///         If RoR2 IS installed AND R2API.StringSerializerExtensions is installed, all the serialization capabilities are enabled.
    ///     </item>
    /// </list>
    /// </summary>
    public static class SerializationMediator
    {
        private static HashSet<Type> _typesWeShouldSerialize = new HashSet<Type>();

        /// <summary>
        /// Returns wether the given field can be serialized in a string format under the current project context
        /// </summary>
        /// <param name="fInfo">The field to serialize</param>
        /// <returns>True if it can be serialized, false otherwise</returns>
        public static bool CanSerializeField(FieldInfo fInfo)
        {
            Type fieldType = fInfo.FieldType;
            if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType) && !CanSerializeType(fieldType))
            {
                return false;
            }
            if (fInfo.IsStatic && fInfo.IsPublic)
            {
                return true;
            }
            return fInfo.GetCustomAttribute<SerializeField>() != null;
        }

        /// <summary>
        /// Checks wether the given type can be serialized in a string format under the current project context
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>True if it can be serialized, otherwise false</returns>
        public static bool CanSerializeType<T>() => CanSerializeType(typeof(T));

        /// <summary>
        /// Checks wether the given type can be serialized in a string format under the current project context
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if it can be serialized, otherwise false</returns>
        public static bool CanSerializeType(Type type)
        {
            if (type.IsEnum)
            {
                return ShouldSerializeEnum();
            }
            return _typesWeShouldSerialize.Contains(type);
        }

        /// <summary>
        /// Serializes a value from field info, this is a modified version of the base game's SerializedValue.Serialize
        /// </summary>
        /// <param name="fInfo">the field info to serialize</param>
        /// <param name="value">The value to serialize</param>
        /// <param name="result">The result, which can be either an object reference, or a serialized string.</param>
        public static void SerializeFromFieldInfo(FieldInfo fInfo, object value, out (UnityEngine.Object objectReference, string serializedString) result)
        {
            result.objectReference = null;
            result.serializedString = "";

            if (typeof(Object).IsAssignableFrom(fInfo.FieldType))
            {
                result.objectReference = (Object)value;
                return;
            }
            if (CanSerializeType(fInfo.FieldType))
            {
                result.serializedString = SerializeInternal(fInfo.FieldType, value);
                return;
            }

            Debug.Log($"Could not serialize field, Unrecognized Type \"{fInfo.FieldType.FullName}\"");
        }

        /// <summary>
        /// Serializes a given value into a string representation under the current project context
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value</returns>
        public static string Serialize<T>(T value)
        {
            var type = typeof(T);
            return Serialize(type, value);
        }

        /// <summary>
        /// Serializes a given value into a string representation under the current project context
        /// </summary>
        /// <param name="type">The type to serialize</param>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value</returns>
        public static string Serialize(Type type, object value)
        {
            if (CanSerializeType(type))
            {
                return SerializeInternal(type, value);
            }
            Debug.Log($"Could not serialize value, Unrecognized Type \"{type.FullName}\"");
            return "";
        }

        internal static string SerializeInternal(Type type, object value) => EditorStringSerializer.Serialize(type, value);

        /// <summary>
        /// Deserializes a given string representation into the selected value under the current project context
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="input">The type's string representation</param>
        /// <returns>The deserialized value</returns>
        public static T Deserialize<T>(string input)
        {
            var type = typeof(T);
            return (T)Deserialize(type, input);
        }

        /// <summary>
        /// Deserializes a given string representation into the selected value under the current project context
        /// </summary>
        /// <param name="type">The type to serialize</typeparam>
        /// <param name="input">The type's string representation</param>
        /// <returns>The deserialized value</returns>
        public static object Deserialize(Type type, string input)
        {
            if (CanSerializeType(type))
            {
                return DeserializeInternal(type, input);
            }
            Debug.Log($"Could not Deserialize string input, Unrecognized Type \"{type.FullName}\"");
            return null;
        }

        internal static object DeserializeInternal(Type type, string serializedValue) => EditorStringSerializer.Deserialize(type, serializedValue);

        private static bool ShouldSerializeEnum()
        {
#if !RISKOFRAIN2
            return true;
#else
#if RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS
            return true;
#else
            return false;
#endif
#endif
        }

        /// <summary>
        /// --o--!!!READ ME!!!--o--!!!READ ME!!!--o--!!!READ ME!!!--o--!!!READ ME!!!--o--!!!READ ME!!!---o----
        /// 
        /// <para>This class is used to directly interact with the EditorStringSerializer within RoR2EditorKit, these methods should only be used for Editor related serialization and NOT runtime serialization.</para>
        /// 
        /// The EditorStringSerializer can serialize the following types:
        /// <list type="bullet">
        ///     <item>[*]<see cref="short"/></item>
        ///     <item>[*]<see cref="ushort"/></item>
        ///     <item>[*]<see cref="int"/></item>
        ///     <item>[*]<see cref="uint"/></item>
        ///     <item>[*]<see cref="long"/></item>
        ///     <item>[*]<see cref="ulong"/></item>
        ///     <item>[*]<see cref="bool"/></item>
        ///     <item>[*]<see cref="float"/></item>
        ///     <item>[*]<see cref="double"/></item>
        ///     <item>[*]<see cref="string"/></item>
        ///     <item>[*]<see cref="Color"/></item>
        ///     <item><see cref="LayerMask"/></item>
        ///     <item>[*]<see cref="Vector2"/></item>
        ///     <item><see cref="Vector2Int"/></item>
        ///     <item>[*]<see cref="Vector3"/></item>
        ///     <item><see cref="Vector3Int"/></item>
        ///     <item><see cref="Vector4"/></item>
        ///     <item><see cref="Rect"/></item>
        ///     <item><see cref="RectInt"/></item>
        ///     <item><see cref="char"/></item>
        ///     <item><see cref="Bounds"/></item>
        ///     <item><see cref="BoundsInt"/></item>
        ///     <item><see cref="Quaternion"/></item>
        ///     <item>[*]<see cref="AnimationCurve"/></item>
        ///     <item><see cref="Enum"/></item>
        ///     <item><see cref="Enum"/> with <see cref="FlagsAttribute"/></item>
        /// </list>
        /// <br>Entries marked with "[*]" means that these are equal to the base game's string serializer, and as such are the only ones available in <see cref="SerializationMediator"/> when RoR2 is installed but not the R2API.StringSerializerExtensions module.</br>
        /// </summary>
        public static class SafetyBypass
        {
            /// <summary>
            /// Checks wether the given type can be serialized in a string format
            /// </summary>
            /// <typeparam name="T">The type to check</typeparam>
            /// <returns>True if it can be serialized, otherwise false</returns>
            public static bool CanSerializeType<T>() => EditorStringSerializer.CanSerializeType<T>();

            /// <summary>
            /// Checks wether the given type can be serialized in a string format
            /// </summary>
            /// <param name="t">The type to check</param>
            /// <returns>True if it can be serialized, otherwise false</returns>
            public static bool CanSerializeType(Type t) => EditorStringSerializer.CanSerializeType(t);

            /// <summary>
            /// Serializes a given value into a string representation
            /// </summary>
            /// <typeparam name="T">The type to serialize</typeparam>
            /// <param name="value">The value to serialize</param>
            /// <returns>The serialized value</returns>
            public static string Serialize<T>(T value) => EditorStringSerializer.Serialize(value);

            /// <summary>
            /// Serializes a given value into a string representation
            /// </summary>
            /// <param name="type">The type to serialize</param>
            /// <param name="value">The value to serialize</param>
            /// <returns>The serialized value</returns>
            public static string Serialize(Type valueType, object value) => EditorStringSerializer.Serialize(valueType, value);

            /// <summary>
            /// Deserializes a given string representation into the selected value
            /// </summary>
            /// <typeparam name="T">The type to serialize</typeparam>
            /// <param name="input">The type's string representation</param>
            /// <returns>The deserialized value</returns>
            public static T Deserialize<T>(string input) => EditorStringSerializer.Deserialize<T>(input);

            /// <summary>
            /// Deserializes a given string representation into the selected value
            /// </summary>
            /// <param name="type">The type to serialize</typeparam>
            /// <param name="input">The type's string representation</param>
            /// <returns>The deserialized value</returns>
            public static object Deserialize(Type type, string input) => EditorStringSerializer.Deserialize(type, input);
        }

        static SerializationMediator()
        {
#if !RISKOFRAIN2
            //No ror2 installed? utilize the editor string serializer directly.
            foreach(var type in EditorStringSerializer._typeToSerializationHandlers.Keys)
            {
                _typesWeShouldSerialize.Add(type);
            }
#else
#if RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS
            //String serializer extensions is installed, EditorStringSerializer handles everything from there. so also use these
            foreach (var type in EditorStringSerializer._typeToSerializationHandlers.Keys)
            {
                _typesWeShouldSerialize.Add(type);
            }
#else
            //Only utilize the officially supported types
            Add<bool>();
            Add<long>();
            Add<ulong>();
            Add<int>();
            Add<uint>();
            Add<short>();
            Add<ushort>();
            Add<float>();
            Add<double>();
            Add<string>();
            Add<Vector2>();
            Add<Vector3>();
            Add<Color>();
            Add<AnimationCurve>();

            void Add<T>() => _typesWeShouldSerialize.Add(typeof(T));
#endif
#endif
        }
    }
}