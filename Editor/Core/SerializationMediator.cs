using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RoR2.Editor
{
    public static class SerializationMediator
    {
        private static HashSet<Type> _typesWeShouldSerialize = new HashSet<Type>();

        public static bool CanSerializeField(FieldInfo fInfo)
        {
            Type fieldType = fInfo.FieldType;
            if(!typeof(UnityEngine.Object).IsAssignableFrom(fieldType) && !CanSerializeType(fieldType))
            {
                return false;
            }
            if(fInfo.IsStatic && fInfo.IsPublic)
            {
                return true;
            }
            return fInfo.GetCustomAttribute<SerializeField>() != null;
        }

        public static bool CanSerializeType<T>() => CanSerializeType(typeof(T));
        public static bool CanSerializeType(Type type)
        {
            if (type.IsEnum)
            {
                return ShouldSerializeEnum();
            }
            return _typesWeShouldSerialize.Contains(type);
        }

        public static void SerializeFromFieldInfo(FieldInfo fInfo, object value, out (UnityEngine.Object objectReference, string serializedString) result)
        {
            result.objectReference = null;
            result.serializedString = "";
            
            if(typeof(Object).IsAssignableFrom(fInfo.FieldType))
            {
                result.objectReference = (Object)value;
                return;
            }
            if(CanSerializeType(fInfo.FieldType))
            {
                result.serializedString = SerializeInternal(fInfo.FieldType, value);
                return;
            }

            Debug.Log($"Could not serialize field, Unrecognized Type \"{fInfo.FieldType.FullName}\"");
        }

        public static string Serialize<T>(T value)
        {
            var type = typeof(T);
            return Serialize(type, value);
        }

        public static string Serialize(Type type, object value)
        {
            if(CanSerializeType(type))
            {
                return SerializeInternal(type, value);
            }
            Debug.Log($"Could not serialize value, Unrecognized Type \"{type.FullName}\"");
            return "";
        }

        internal static string SerializeInternal(Type type, object value) => EditorStringSerializer.Serialize(type, value);

        public static T Deserialize<T>(string input)
        {
            var type = typeof(T);
            return (T)Deserialize(type, input);
        }

        public static object Deserialize(Type type, string input)
        {
            if(CanSerializeType(type))
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

        static SerializationMediator()
        {
            //No ror2 installed? utilize the editor string serializer directly.
#if !RISKOFRAIN2
            foreach(var type in EditorStringSerializer._typeToSerializationHandlers.Keys)
            {
                _typesWeShouldSerialize.Add(type);
            }
#else
            //String serializer extensions is installed, EditorStringSerializer handles everything from there. so also use these
#if RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS
            foreach (var type in EditorStringSerializer._typeToSerializationHandlers.Keys)
            {
                _typesWeShouldSerialize.Add(type);
            }
            //Only utilize the officially supported types
#else
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