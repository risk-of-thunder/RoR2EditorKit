using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// a Class containing a plethora of utility methods for drawing IMGUI based UI's
    /// </summary>
    public static class IMGUIUtil
    {
        private static readonly Dictionary<Type, FieldDrawHandler> _typeDrawers = new Dictionary<Type, FieldDrawHandler>();

        private static FieldDrawHandler _enumFlagsHandler;
        private static FieldDrawHandler _enumTypeHandler;

        /// <summary>
        /// Draws <paramref name="property"/> within an <see cref="EditorGUI.BeginChangeCheck"/> and calls <paramref name="onValueChanged"/> when the <paramref name="property"/>'s value changes.
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="onValueChanged">The method to invoke when the value changes.</param>
        public static void DrawCheckableProperty(SerializedProperty property, Action<SerializedProperty> onValueChanged)
        {
            DrawCheckableProperty(property, onValueChanged, property.GetGUIContent(), true);
        }


        /// <summary>
        /// Draws <paramref name="property"/> within an <see cref="EditorGUI.BeginChangeCheck"/> and calls <paramref name="onValueChanged"/> when the <paramref name="property"/>'s value changes.
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="onValueChanged">The method to invoke when the value changes.</param>
        /// <param name="drawChildren">Wether children of <paramref name="property"/> should be drawn too</param>
        /// <returns>True if the property has children, is expanded, and drawChildren is set to false, otherwise false.</returns>
        public static bool DrawCheckableProperty(SerializedProperty property, Action<SerializedProperty> onValueChanged, bool drawChildren)
        {
            return DrawCheckableProperty(property, onValueChanged, property.GetGUIContent(), drawChildren);
        }

        /// <summary>
        /// Draws <paramref name="property"/> within an <see cref="EditorGUI.BeginChangeCheck"/> and calls <paramref name="onValueChanged"/> when the <paramref name="property"/>'s value changes.
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="onValueChanged">The method to invoke when the value changes.</param>
        /// <param name="label">The label to display for the property</param>
        public static void DrawCheckableProperty(SerializedProperty property, Action<SerializedProperty> onValueChanged, GUIContent label)
        {
            DrawCheckableProperty(property, onValueChanged, label, true);
        }


        /// <summary>
        /// Draws <paramref name="property"/> within an <see cref="EditorGUI.BeginChangeCheck"/> and calls <paramref name="onValueChanged"/> when the <paramref name="property"/>'s value changes.
        /// </summary>
        /// <param name="property">The property to draw</param>
        /// <param name="onValueChanged">The method to invoke when the value changes.</param>
        /// <param name="label">The label to display for the property</param>
        /// <param name="drawChildren">Wether children of <paramref name="property"/> should be drawn too</param>
        /// <returns>True if the property has children, is expanded, and drawChildren is set to false, otherwise false.</returns>
        public static bool DrawCheckableProperty(SerializedProperty property, Action<SerializedProperty> onValueChanged, GUIContent label, bool drawChildren)
        {
            EditorGUI.BeginChangeCheck();
            var v = EditorGUILayout.PropertyField(property, label, drawChildren);
            if (EditorGUI.EndChangeCheck())
            {
                onValueChanged(property);
            }
            return v;
        }

        /// <summary>
        /// Draws the given property with a snug label, useful when grouping multiple property fields inside a horizontal group
        /// </summary>
        /// <param name="property">The proeprty to draw with a snug label</param>
        /// <param name="includeChildren">Wether children should be included or not</param>
        /// <param name="extraWidth">Extra padding that'll be given to the label's width</param>
        public static void PropertyFieldWithSnugLabel(SerializedProperty property, bool includeChildren = true, float extraWidth = 20)
        {
            var origLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(property.displayName, property.tooltip)).x + extraWidth;
            EditorGUILayout.PropertyField(property, includeChildren);
            EditorGUIUtility.labelWidth = origLabelWidth;
        }

        /// <summary>
        /// Checks if the type specified in <paramref name="type"/> can be drawn using IMGUI
        /// </summary>
        /// <returns>True if the type can be drawn, false otherwise</returns>
        public static bool CanDrawFieldFromType(Type type)
        {
            return _typeDrawers.ContainsKey(type) || type.IsEnum;
        }

        /// <summary>
        /// Draws a field of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of field to draw</typeparam>
        /// <param name="content">The label for the field</param>
        /// <param name="fieldValue">The initial value of the field</param>
        /// <param name="newValue">The new value of the field</param>
        /// <returns>True if the value has changed, false otherwise</returns>
        public static bool DrawFieldOfType<T>(GUIContent content, T fieldValue, out T newValue)
        {
            var boolValue = DrawFieldOfType(typeof(T), content, fieldValue, out var nValue);
            newValue = (T)nValue;
            return boolValue;
        }

        /// <summary>
        /// Draws a field of type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of field to draw</param>
        /// <param name="content">The label for the field</param>
        /// <param name="fieldValue">The initial value of the field</param>
        /// <param name="newValue">The new value of the field</param>
        /// <returns>True if the value has changed, false otherwise</returns>
        public static bool DrawFieldOfType(Type type, GUIContent content, object fieldValue, out object newValue)
        {
            if (!CanDrawFieldFromType(type))
            {
                EditorGUILayout.LabelField($"Type {type.FullName} cannot be drawn");
                newValue = default;
                return false;
            }

            if (type.IsEnum)
            {
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    EditorGUI.BeginChangeCheck();
                    newValue = _enumFlagsHandler(content, fieldValue);
                    return EditorGUI.EndChangeCheck();
                }
                EditorGUI.BeginChangeCheck();
                newValue = _enumTypeHandler(content, fieldValue);
                return EditorGUI.EndChangeCheck();
            }
            EditorGUI.BeginChangeCheck();
            newValue = _typeDrawers[type](content, fieldValue);
            return EditorGUI.EndChangeCheck();
        }

        /// <summary>
        /// Creates an IMGUI field for the specified EditorSetting
        /// </summary>
        /// <typeparam name="T">The type of the editor setting's value</typeparam>
        /// <param name="setting">The setting from which we'll get the values</param>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value in case a setting of name <paramref name="settingName"/> doesnt exist</param>
        /// <returns>True if the setting was changed, false otherwise</returns>
        public static bool CreateFieldForSetting<T>(EditorSettingCollection setting, string settingName, T defaultValue = default)
        {
            Type type = typeof(T);
            return CreateFieldForSetting(setting, settingName, type, defaultValue);
        }

        /// <summary>
        /// Creates an IMGUI field for the specified EditorSetting
        /// </summary>
        /// <param name="valueType">The type of the editor setting's value</param>
        /// <param name="setting">The setting from which we'll get the values</param>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value in case a setting of name <paramref name="settingName"/> doesnt exist</param>
        /// <returns>True if the setting was changed, false otherwise</returns>
        public static bool CreateFieldForSetting(EditorSettingCollection setting, string settingName, Type valueType, object defaultValue = default)
        {
            if (!CanDrawFieldFromType(valueType) || !EditorStringSerializer.CanSerializeType(valueType))
            {
                EditorGUILayout.LabelField($"Cannot create a field for setting with a value type of {valueType.FullName}");
                return false;
            }

            object value = setting.GetOrCreateSetting(settingName, defaultValue);
            if (DrawFieldOfType(valueType, new GUIContent(ObjectNames.NicifyVariableName(settingName)), value, out var newValue))
            {
                setting.SetSettingValue(settingName, newValue);
                return true;
            }
            return false;
        }

        private delegate object FieldDrawHandler(GUIContent guiContent, object value);

        static IMGUIUtil()
        {
            Add<short>((l, v) =>
            {
                EditorGUI.BeginChangeCheck();
                var valueAsInt = EditorGUILayout.IntField(l, (short)v);
                if (EditorGUI.EndChangeCheck())
                {
                    if (valueAsInt > short.MaxValue)
                    {
                        valueAsInt = short.MaxValue;
                    }
                    else if (valueAsInt < short.MinValue)
                    {
                        valueAsInt = short.MinValue;
                    }
                }
                return (short)valueAsInt;
            });
            Add<ushort>((l, v) =>
            {
                EditorGUI.BeginChangeCheck();
                var valueAsInt = EditorGUILayout.IntField(l, (ushort)v);
                if (EditorGUI.EndChangeCheck())
                {
                    if (valueAsInt > ushort.MaxValue)
                    {
                        valueAsInt = ushort.MaxValue;
                    }
                    else if (valueAsInt < 0)
                    {
                        valueAsInt = 0;
                    }
                }
                return (ushort)valueAsInt;
            });
            Add<int>((l, v) => EditorGUILayout.IntField(l, (int)v));
            Add<uint>((l, v) =>
            {
                EditorGUI.BeginChangeCheck();
                var valueAsLong = EditorGUILayout.LongField(l, (uint)v);
                if (EditorGUI.EndChangeCheck())
                {
                    if (valueAsLong > uint.MaxValue)
                    {
                        valueAsLong = uint.MaxValue;
                    }
                    else if (valueAsLong < 0)
                    {
                        valueAsLong = 0;
                    }
                }
                return (uint)valueAsLong;
            });
            Add<long>((l, v) => EditorGUILayout.LongField(l, (long)v));
            Add<ulong>((l, v) =>
            {
                EditorGUI.BeginChangeCheck();
                var valueAsLong = EditorGUILayout.LongField(l, (long)(ulong)v);
                if (EditorGUI.EndChangeCheck())
                {
                    if (valueAsLong < 0)
                    {
                        valueAsLong = 0;
                    }
                }
                return valueAsLong;
            });
            Add<bool>((l, v) => EditorGUILayout.Toggle(l, (bool)v));
            Add<float>((l, v) => EditorGUILayout.FloatField(l, (float)v));
            Add<double>((l, v) => EditorGUILayout.DoubleField(l, (double)v));
            Add<string>((l, v) => EditorGUILayout.TextField(l, (string)v));
            Add<Color>((l, v) => EditorGUILayout.ColorField(l, (Color)v));
            Add<LayerMask>((l, v) => EditorGUILayout.MaskField(l, ((LayerMask)v).value, InternalEditorUtility.layers));
            Add<Vector2>((l, v) => EditorGUILayout.Vector2Field(l, (Vector2)v));
            Add<Vector3>((l, v) => EditorGUILayout.Vector3Field(l, (Vector3)v));
            Add<Vector4>((l, v) => EditorGUILayout.Vector4Field(l, (Vector4)v));
            Add<Vector2Int>((l, v) => EditorGUILayout.Vector2IntField(l, (Vector2Int)v));
            Add<Vector3Int>((l, v) => EditorGUILayout.Vector3IntField(l, (Vector3Int)v));
            Add<Rect>((l, v) => EditorGUILayout.RectField(l, (Rect)v));
            Add<RectInt>((l, v) => EditorGUILayout.RectIntField(l, (RectInt)v));
            Add<char>((l, v) =>
            {
                EditorGUI.BeginChangeCheck();
                var stringValue = EditorGUILayout.TextField(l, (string)v);
                if (EditorGUI.EndChangeCheck())
                {
                    if (stringValue.Length > 0)
                    {
                        return stringValue[0];
                    }
                }
                return (char)v;
            });
            Add<Bounds>((l, v) => EditorGUILayout.BoundsField(l, (Bounds)v));
            Add<BoundsInt>((l, v) => EditorGUILayout.BoundsIntField(l, (BoundsInt)v));
            Add<Quaternion>((l, v) =>
            {
                Vector3 asEuler = Quaternion.Euler((Vector3)v).eulerAngles;
                EditorGUI.BeginChangeCheck();
                var val = EditorGUILayout.Vector3Field(l, asEuler);
                return Quaternion.Euler(val);
            });
            Add<AnimationCurve>((l, v) => EditorGUILayout.CurveField(l, (AnimationCurve)v));

            _enumFlagsHandler = (l, v) => EditorGUILayout.EnumFlagsField(l, (Enum)v);
            _enumTypeHandler = (l, v) => EditorGUILayout.EnumPopup(l, (Enum)v);

            void Add<T>(FieldDrawHandler handler) => _typeDrawers.Add(typeof(T), handler);
        }
    }
}