using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// Various extension methods from R2EK
    /// </summary>
    public static class R2EKExtensions
    {
        /// <summary>
        /// Calls <see cref="string.IsNullOrEmpty(string)"/> and <see cref="string.IsNullOrWhiteSpace(string)"/> on the given string and computes the logical OR value
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if the string is null, empty or whitespace, otherwise false</returns>
        public static bool IsNullOrEmptyOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Deconstruction utility for key value pairs
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static GUIContent GetGUIContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        // <author>
        //   douduck08: https://github.com/douduck08
        //   Use Reflection to get instance of Unity's SerializedProperty in Custom Editor.
        //   Modified codes from 'Unity Answers', in order to apply on nested List<T> or Array. 
        //   
        //   Original author: HiddenMonk & Johannes Deml
        //   Ref: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
        // </author>

        //Note: a bunch of this code could probably be trimmed down utilizing TryGetFieldInfoFromProperty, but i cba to test that out rn.
        #region GetValueFromProperty
        /// <summary>
        /// Obtains the Value stored within <paramref name="property"/> utilizing Reflection.
        /// <br></br>
        /// The value is Boxed inside an object, you may utilize pattern matching to unbox it.
        /// </summary>
        /// <param name="property">The SerializedProperty from which we want to get it's value</param>
        /// <returns>The value boxed as an object</returns>
        public static object GetValue(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            Regex rgx = new Regex(@"\[\d+\]");
            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }
            return obj;
        }

        /// <summary>
        /// Obtains the Value stored within <paramref name="property"/> utilizing Reflection.
        /// </summary>
        /// <param name="property">The SerializedProperty from which we want to get it's value</param>
        /// <returns>The value stored within <paramref name="property"/></returns>
        public static T GetValue<T>(this SerializedProperty property)
        {
            return (T)GetValue(property);
        }

        /// <summary>
        /// Sets the Value thats stored within <paramref name="property"/> to <paramref name="value"/>, utilizing Reflection.
        /// </summary>
        /// <param name="property">The SerializedProperty from which we want to set it's value</param>
        /// <param name="value">The new value</param>
        /// <returns>Wether the value was set succesfully</returns>
        public static bool SetValue(this SerializedProperty property, object value)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            Regex rgx = new Regex(@"\[\d+\]");
            for (int i = 0; i < fieldStructure.Length - 1; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            string fieldName = fieldStructure.Last();
            if (fieldName.Contains("["))
            {
                int index = Convert.ToInt32(new string(fieldName.Where(c => char.IsDigit(c)).ToArray()));
                return SetFieldValueWithIndex(rgx.Replace(fieldName, ""), obj, index, value);
            }
            else
            {
                return SetFieldValue(fieldName, obj, value);
            }
        }

        /// <summary>
        /// Sets the Value thats stored within <paramref name="property"/> to <paramref name="value"/>, utilizing Reflection.
        /// </summary>
        /// <param name="property">The SerializedProperty from which we want to set it's value</param>
        /// <param name="value">The new value</param>
        /// <returns>Wether the value was set succesfully</returns>
        public static bool SetValue<T>(this SerializedProperty property, T value)
        {
            return SetValue(property, (object)value);
        }

        private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            return default;
        }

        private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    if(list is IList ilist)
                    {
                        return ilist[index];
                    }
                    return ((object[])list)[index];
                }
                else if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }
            return default;
        }

        private static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            return false;
        }

        private static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    ((object[])list)[index] = value;
                    return true;
                }
                else if (value is IEnumerable)
                {
                    ((IList)list)[index] = value;
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Returns the Parent <see cref="SerializedProperty"/> of <paramref name="serializedProperty"/>
        /// </summary>
        /// <param name="serializedProperty">The <see cref="SerializedProperty"/> from which we'll get it's parent</param>
        /// <param name="parentProperty">The obtained parent property</param>
        /// <returns>False if <paramref name="serializedProperty"/> has no parent property, otherwise returns True"/></returns>
        public static bool TryGetParentProperty(this SerializedProperty serializedProperty, out SerializedProperty parentProperty)
        {
            parentProperty = null;
            var propertyPaths = serializedProperty.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return false;
            }

            parentProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array" && propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                {
                    var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentProperty = parentProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
                else
                {
                    parentProperty = parentProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return true;
        }

        private static MethodInfo getFieldInfoFromProperty;

        /// <summary>
        /// Retrieves the <see cref="FieldInfo"/> that <paramref name="property"/> is representing
        /// </summary>
        /// <param name="property">The SerializedProperty to get it's FieldInfo</param>
        /// <param name="fieldInfo">The obtained FieldInfo</param>
        /// <returns>True if the FieldInfo is obtained succesfully, otherwise false</returns>
        public static bool GetFieldInfoFromProperty(SerializedProperty property, out FieldInfo fieldInfo)
        {
            if (getFieldInfoFromProperty == null)
            {
                var scriptAttributeUtilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
                getFieldInfoFromProperty = scriptAttributeUtilityType.GetMethod(nameof(GetFieldInfoFromProperty), BindingFlags.NonPublic | BindingFlags.Static);
            }

            fieldInfo = (FieldInfo)getFieldInfoFromProperty.Invoke(null, new object[]
            {
                property,
                null
            });
            return fieldInfo != null;
        }
    }
}