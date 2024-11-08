using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace RoR2.Editor
{
    /// <summary>
    /// Contains Reflection related utilities
    /// </summary>
    [InitializeOnLoad]
    public static class ReflectionUtils
    {
        /// <summary>
        /// Shorthand for (BindingFlags)~0
        /// </summary>
        public static readonly BindingFlags all = (BindingFlags)~0;

        /// <summary>
        /// All the types found within the current domain
        /// </summary>
        public static readonly ReadOnlyCollection<Type> allTypes;

        /// <summary>
        /// Returns all the types from <paramref name="assembly"/>, if <see cref="ReflectionTypeLoadException"/> gets thrown, it'll return the available types.
        /// </summary>
        /// <param name="assembly">The assembly to get it's types</param>
        /// <returns>An array of all the Types available from the assembly</returns>
        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            Type[] result = null;
            try
            {
                result = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                result = e.Types;
            }
            return result ?? Array.Empty<Type>();
        }
        static ReflectionUtils()
        {
            allTypes = new ReadOnlyCollection<Type>(AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesSafe).ToList());
        }
    }
}