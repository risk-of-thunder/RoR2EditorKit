using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace RoR2EditorKit
{
    public static class TypeCacheRequester
    {
        private static Type[] allTypes = Array.Empty<Type>(); 
        private static Dictionary<Type, Type[]> typeToDerivedTypeCollection = new Dictionary<Type, Type[]>();
        private static Dictionary<Type, Type[]> attributeToTypeCollection = new Dictionary<Type, Type[]>();
        private static Dictionary<Type, MethodInfo[]> attributeToMethodCollection = new Dictionary<Type, MethodInfo[]>();

        public static Type[] GetAllTypes(bool allowAbstractTypes)
        {
            if(allTypes.Length == 0)
            {
                allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypesSafe())
                    .OrderBy(t => t.FullName)
                    .ToArray();
            }
            return allTypes.Where(t => allowAbstractTypes ? true : !t.IsAbstract).ToArray();
        }

        public static Type[] GetTypesDerivedFrom<T>(bool allowAbstractTypes) => GetTypesDerivedFromInternal(typeof(T), allowAbstractTypes);

        public static Type[] GetTypesDerivedFrom(Type type, bool allowAbstractTypes) => GetTypesDerivedFromInternal(type, allowAbstractTypes);

        public static Type[] GetTypesWithAttribute<TAttribute>(bool allowAbstractTypes) where TAttribute : Attribute => GetTypesWithAttributeInternal(typeof(TAttribute), allowAbstractTypes);

        public static Type[] GetTypesWithAttribute(Type attributeType, bool allowAbstractTypes) => GetTypesWithAttributeInternal(attributeType, allowAbstractTypes);

        public static MethodInfo[] GetMethodInfosWithAttribute<TAttribute>() where TAttribute : Attribute => GetMethodInfosWithAttributeInternal(typeof(TAttribute));

        public static MethodInfo[] GetMethodInfosWithAttribute(Type attributeType) => GetMethodInfosWithAttributeInternal(attributeType);

        private static Type[] GetTypesDerivedFromInternal(Type type, bool allowAbstractTypes)
        {
            if(!typeToDerivedTypeCollection.ContainsKey(type))
            {
                typeToDerivedTypeCollection[type] = TypeCache.GetTypesDerivedFrom(type)
                    .OrderBy(t => t.FullName)
                    .ToArray();
            }
            return typeToDerivedTypeCollection[type].Where(t => allowAbstractTypes ? true : !t.IsAbstract).ToArray();
        }

        private static Type[] GetTypesWithAttributeInternal(Type attributeType, bool allowAbstractTypes)
        {
            if(!attributeToTypeCollection.ContainsKey(attributeType))
            {
                attributeToTypeCollection[attributeType] = TypeCache.GetTypesWithAttribute(attributeType)
                    .OrderBy(t => t.FullName)
                    .ToArray();
            }
            return attributeToTypeCollection[attributeType].Where(t => allowAbstractTypes ? true : !t.IsAbstract).ToArray();
        }

        private static MethodInfo[] GetMethodInfosWithAttributeInternal(Type attributeType)
        {
            if(!attributeToMethodCollection.ContainsKey(attributeType))
            {
                attributeToMethodCollection[attributeType] = TypeCache.GetMethodsWithAttribute(attributeType)
                    .OrderBy(m => $"{m.DeclaringType.FullName}.{m.Name}")
                    .ToArray();
            }
            return attributeToMethodCollection[attributeType];
        }
    }
}