using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="SerializationMediator"/> is a class that mediates the String serialization capabilities built into RoR2EditorKit. it's used for managing what fields can be serialized by the <see cref="SerializedFieldCollectionElement"/>. and the methods for creating control elements within <see cref="VisualElementUtil"/> 
    /// <br>You can bypass this mediator by using <see cref="EditorStringSerializer"/> directly</br>
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
        /// Adds a new type to serialize to the SerializationMediator.
        /// <br></br>
        /// Logs an error if the <see cref="EditorStringSerializer"/> doesnt support the Type.
        /// </summary>
        /// <typeparam name="T">The type to add to the mediator</typeparam>
        public static void AddTypeToSerialize<T>() => AddTypeToSerialize(typeof(T));

        /// <summary>
        /// Adds a new type to serialize to the SerializationMediator.
        /// <br></br>
        /// Logs an error if the <see cref="EditorStringSerializer"/> doesnt support the Type.
        /// </summary>
        /// <param name="type">The type to add to the mediator</param>
        public static void AddTypeToSerialize(Type type)
        {
            if(!EditorStringSerializer.CanSerializeType(type))
            {
                RoR2EKLog.Error($"Cannot serialize {type} as the internal EditorStringSerializer does not have support for it.");
                return;
            }

            _typesWeShouldSerialize.Add(type);
        }

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

            RoR2EKLog.Debug($"Could not serialize field, Unrecognized Type \"{fInfo.FieldType.FullName}\"");
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
            RoR2EKLog.Debug($"Could not serialize value, Unrecognized Type \"{type.FullName}\"");
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
            RoR2EKLog.Debug($"Could not Deserialize string input, Unrecognized Type \"{type.FullName}\"");
            return null;
        }

        internal static object DeserializeInternal(Type type, string serializedValue) => EditorStringSerializer.Deserialize(type, serializedValue);

        private static bool ShouldSerializeEnum()
        {
#if !R2EK_ROR2_INSTALLED
            return true;
#else
#if R2EK_STRINGSERIALIZEREXTENSIONS_INSTALLED
            return true;
#else
            return false;
#endif
#endif
        }

        [Obsolete("If you wish to bypass the mediator, use EditorStringSerializer directly.")]
        public static class SafetyBypass
        {
            public static bool CanSerializeType<T>() => EditorStringSerializer.CanSerializeType<T>();

            public static bool CanSerializeType(Type t) => EditorStringSerializer.CanSerializeType(t);

            public static string Serialize<T>(T value) => EditorStringSerializer.Serialize(value);

            public static string Serialize(Type valueType, object value) => EditorStringSerializer.Serialize(valueType, value);

            public static T Deserialize<T>(string input) => EditorStringSerializer.Deserialize<T>(input);

            public static object Deserialize(Type type, string input) => EditorStringSerializer.Deserialize(type, input);
        }

        static SerializationMediator()
        {
#if !R2EK_ROR2_INSTALLED
            //No ror2 installed? utilize the editor string serializer directly.
            foreach(var type in EditorStringSerializer.GetSerializableTypes())
            {
                AddTypeToSerialize(type);
            }
#elif R2EK_STRINGSERIALIZEREXTENSIONS_INSTALLED
            //String serializer extensions is installed, EditorStringSerializer handles everything from there. so also use these
            foreach (var type in EditorStringSerializer.GetSerializableTypes())
            {
                AddTypeToSerialize(type);
            }
#else
            //Only utilize the officially supported types
            AddTypeToSerialize<bool>();
            AddTypeToSerialize<long>();
            AddTypeToSerialize<ulong>();
            AddTypeToSerialize<int>();
            AddTypeToSerialize<uint>();
            AddTypeToSerialize<short>();
            AddTypeToSerialize<ushort>();
            AddTypeToSerialize<float>();
            AddTypeToSerialize<double>();
            AddTypeToSerialize<string>();
            AddTypeToSerialize<Vector2>();
            AddTypeToSerialize<Vector3>();
            AddTypeToSerialize<Color>();
            AddTypeToSerialize<AnimationCurve>();
#endif
        }
    }
}