using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    [InitializeOnLoad]
    public static class ReflectionUtils
    {
        public static readonly ReadOnlyCollection<Type> allTypes;
        public static Type[] GetTypesSafe(this Assembly assembly)
        {
            Type[] result = null;
            try
            {
                result = assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException e)
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