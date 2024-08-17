using System;
using System.Linq;
using System.Reflection;

namespace RoR2EditorKit
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Gets all the types from an assembly safely by getting the types from a ReflectionTypeLoadException if one is thrown
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="assemblyTypes"></param>
        /// <returns>true if a ReflectionTypeLoadException was caught</returns>
        public static bool GetTypesSafe(Assembly assembly, out Type[] assemblyTypes)
        {
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException re)
            {
                assemblyTypes = re.Types.Where(t => t != null).ToArray();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all the types from an assembly safely by getting the types from a ReflectionTypeLoadException if one is thrown
        /// </summary>
        /// <returns>The types of the assembly</returns>
        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException re)
            {
                types = re.Types.Where(t => t != null).ToArray();
            }
            return types;
        }

        /// <summary>
        /// Checks if <paramref name="givenType"/> is assignable to the type specified in <paramref name="genericType"/>
        /// </summary>
        /// <returns>True if the type <paramref name="givenType"/> can be assigned to <paramref name="genericType"/>, false otherwise</returns>
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}