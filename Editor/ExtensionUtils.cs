using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit
{
    /// <summary>
    /// Class holding a multitude of extension methods.
    /// </summary>
    public static class ExtensionUtils
    {
        #region String Extensions
        /// <summary>
        /// Ensures that the string object is not Null, Empty or WhiteSpace.
        /// </summary>
        /// <param name="text">The string object to check</param>
        /// <returns>True if the string object is not Null, Empty or Whitespace, false otherwise.</returns>
        public static bool IsNullOrEmptyOrWhitespace(this string text)
        {
            return string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text);
        }
        #endregion

        #region KeyValuePair Extensions
        /// <summary>
        /// Extension to allow tuple style deconstruction of keys and values when enumerating a dictionary.
        /// Example: foreach(var (key, value) in myDictionary)
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="kvp"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
        #endregion

        #region SerializedProperties/Objects  Extensions
        /// <summary>
        /// Returns the serialized property that's bound to this bindable element.
        /// </summary>
        /// <param name="objField">The BindableElement that has a bounded property</param>
        /// <param name="objectBound">The SerializedObject that has the objectField's property binding path.</param>
        /// <returns>The serialized property</returns>
        /// <exception cref="NullReferenceException">when the objField does not have a bindingPath set.</exception>
        public static SerializedProperty GetBindedProperty(this IBindable bindableElement, SerializedObject objectBound)
        {
            if (bindableElement.bindingPath.IsNullOrEmptyOrWhitespace())
                throw new NullReferenceException($"{bindableElement} does not have a bindingPath set");

            return objectBound.FindProperty(bindableElement.bindingPath);
        }

        /// <summary>
        /// Obtains a List of all the top layer serialized properties from a serialized object.
        /// </summary>
        /// <param name="serializedObject">The serialized object to get the children</param>
        /// <returns>A List of all the top layer serialized properties</returns>
        public static List<SerializedProperty> GetVisibleChildren(this SerializedObject serializedObject)
        {
            List<SerializedProperty> list = new List<SerializedProperty>();
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    list.Add(serializedObject.FindProperty(iterator.propertyPath));
                }

                enterChildren = false;
            }
            return list;
        }

        /// <summary>
        /// Runs SerializedObject.ApplyModifiedProperties(), then updates the representation.
        /// </summary>
        /// <param name="serializedObject"></param>
        public static void ApplyAndUpdate(this SerializedObject serializedObject)
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        /// <summary>
        /// Finds the parent property of the selected serialized property
        /// </summary>
        public static SerializedProperty GetParentProperty(this SerializedProperty serializedProperty)
        {
            var propertyPaths = serializedProperty.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return default;
            }

            var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array")
                {
                    if (index + 1 == propertyPaths.Length - 1)
                    {
                        // reached the end
                        break;
                    }
                    if (propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                    {
                        var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                        var arrayIndex = int.Parse(match.Groups[1].Value);
                        parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                        index++;
                    }
                }
                else
                {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return parentSerializedProperty;
        }

        #region Douduck08 Extensions

        // <author>
        //   douduck08: https://github.com/douduck08
        //   Use Reflection to get instance of Unity's SerializedProperty in Custom Editor.
        //   Modified codes from 'Unity Answers', in order to apply on nested List<T> or Array. 
        //   
        //   Original author: HiddenMonk & Johannes Deml
        //   Ref: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
        // </author>
        public static T GetValue<T>(this SerializedProperty property)
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
            return (T)obj;
        }

        public static bool SetValue<T>(this SerializedProperty property, T value)
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
                Debug.Log(value);
                return SetFieldValue(fieldName, obj, value);
            }
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
                    return ((object[])list)[index];
                }
                else if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }
            return default;
        }

        public static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            return false;
        }

        public static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
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
        #endregion

        #region GameObject Extensions
        /// <summary>
        /// Retrieves the root GameObject, aka the first object in the hierarchy of a prefab
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>The root GameObject</returns>
        public static GameObject GetRootObject(this GameObject obj)
        {
            return obj.transform.root.gameObject;
        }
        #endregion
    }
}
