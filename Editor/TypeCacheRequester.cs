using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace RoR2EditorKit
{
    /// <summary>
    /// The TypeCacheRequester is a static wrapper class for unity's <see cref="TypeCache"/> class, it effectively stores the results from it's types into a Dictionary.
    /// <para>These types requested by the TypeCacheRequester are ordered by their FullNames</para>
    /// </summary>
    public static class TypeCacheRequester
    {
        private static Type[] allTypes = Array.Empty<Type>(); 
        private static Dictionary<Type, Type[]> typeToDerivedTypeCollection = new Dictionary<Type, Type[]>();
        private static Dictionary<Type, Type[]> attributeToTypeCollection = new Dictionary<Type, Type[]>();
        private static Dictionary<Type, MethodInfo[]> attributeToMethodCollection = new Dictionary<Type, MethodInfo[]>();

        /// <summary>
        /// Retrieves all Types in the current AppDomain, sorted by their full name (Namespace + TypeName)
        /// </summary>
        /// <param name="allowAbstractTypes">Wether AbstractTypes are included in the array</param>
        /// <returns>An array of all types in the app domain.</returns>
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

        /// <summary>
        /// Returns all the Types in the current AppDomain that are derived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type that will be used in the search, The types returned by this method will inherit from this Type </typeparam>
        /// <param name="allowAbstractTypes">Wether Abstract Types are included in the array</param>
        /// <returns>An array of types that derive from <typeparamref name="T"/></returns>
        public static Type[] GetTypesDerivedFrom<T>(bool allowAbstractTypes) => GetTypesDerivedFromInternal(typeof(T), allowAbstractTypes);
        /// <summary>
        /// Returns all the Types in the current AppDomain that are derived from <typeparamref name="T"/>
        /// </summary>
        /// <param name="type">The type that will be used in the search, The types returned by this method will inherit from this Type</param>
        /// <param name="allowAbstractTypes">Wether Abstract Types are included in the array</param>
        /// <returns>An array of types that derive from <typeparamref name="T"/></returns>
        public static Type[] GetTypesDerivedFrom(Type type, bool allowAbstractTypes) => GetTypesDerivedFromInternal(type, allowAbstractTypes);

        /// <summary>
        /// Returns all the types in the current app domain that have the Attribute specified in <typeparamref name="TAttribute"/> applied to them.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type that will be used in the search, the types returned by this method will have this attribute.</typeparam>
        /// <param name="allowAbstractTypes">Wether Abstract Types are included in the array</param>
        /// <returns>An array of types that contain the attribute specified in <typeparamref name="TAttribute"/></returns>
        public static Type[] GetTypesWithAttribute<TAttribute>(bool allowAbstractTypes) where TAttribute : Attribute => GetTypesWithAttributeInternal(typeof(TAttribute), allowAbstractTypes);

        /// <summary>
        /// Returns all the types in the current app domain that have the Attribute specified in <typeparamref name="TAttribute"/> applied to them.
        /// </summary>
        /// <param name="attributeType">The attribute type that will be used in the search, the types returned by this method will have this attribute.</param>
        /// <param name="allowAbstractTypes">Wether Abstract Types are included in the array</param>
        /// <returns>An array of types that contain the attribute specified in <paramref name="attributeType"/></returns>
        public static Type[] GetTypesWithAttribute(Type attributeType, bool allowAbstractTypes) => GetTypesWithAttributeInternal(attributeType, allowAbstractTypes);

        /// <summary>
        /// Returns all the methods in the current app domain that have the attribute specified in <typeparamref name="TAttribute"/> applied to them.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type that will be used in the search, the MethodInfos returned by this method will have this attribute</typeparam>
        /// <returns>An array of MethodInfos that contain the attribute specified in <typeparamref name="TAttribute"/>, the methodInfos are sorted by Namespace, DeclaringType and MethodName</returns>
        public static MethodInfo[] GetMethodInfosWithAttribute<TAttribute>() where TAttribute : Attribute => GetMethodInfosWithAttributeInternal(typeof(TAttribute));

        /// <summary>
        /// Returns all the methods in the current app domain that have the attribute specified in <typeparamref name="TAttribute"/> applied to them.
        /// </summary>
        /// <param name="attributeType">The attribute type that will be used in the search, the MethodInfos returned by this method will have this attribute</param>
        /// <returns>An array of MethodInfos that contain the attribute specified in <paramref name="attributeType"/>, the methodInfos are sorted by Namespace, DeclaringType and MethodName</returns>
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