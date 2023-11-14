using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit
{
    /// <summary>
    /// A class containing a plethora of utility methods for drawing IMGUI based UI's
    /// </summary>
    public static class IMGUIUtil
    {
        private delegate object FieldDrawHandler(GUIContent labelTooltip, object value);
        private static readonly Dictionary<Type, FieldDrawHandler> typeDrawers = new Dictionary<Type, FieldDrawHandler>();

#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
        private static FieldDrawHandler enumFlagsTypeHandler;
        private static FieldDrawHandler enumTypeHandler;
#endif

        /// <summary>
        /// Draws the serialized property specified in <paramref name="prop"/> inside a <see cref="EditorGUI.BeginChangeCheck"/> block.
        /// </summary>
        /// <param name="prop">The property to draw</param>
        /// <param name="propertyChangedAction">The action to trigger when the property's value changes</param>
        /// <param name="includeChildren">Wether the property's children are also drawn</param>
        /// <param name="label">A custom label for the field</param>
        public static void DrawCheckableProperty(SerializedProperty prop, Action propertyChangedAction, bool includeChildren = false, GUIContent label = null)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop, label ?? GetLabelFromProperty(prop), includeChildren);
            prop.serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                propertyChangedAction();
            }
        }

        /// <summary>
        /// <inheritdoc cref="DrawCheckableProperty(SerializedProperty, Action, bool, GUIContent)"/>
        /// </summary>
        public static void DrawCheckableProperty(SerializedProperty prop, Action<SerializedProperty> propertyChangedAction, bool includeChildren = true, GUIContent label = null)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop, label ?? GetLabelFromProperty(prop), includeChildren);
            prop.serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                propertyChangedAction(prop);
            }
        }

        /// <summary>
        /// Returns a label from a property
        /// </summary>
        /// <returns>The label for the property</returns>
        public static GUIContent GetLabelFromProperty(SerializedProperty prop)
        {
            return new GUIContent(prop.displayName, prop.tooltip);
        }

        /// <summary>
        /// Creates a button that only becomes enabled if <paramref name="condition"/> is true
        /// </summary>
        /// <param name="condition">The condition for the button to be clickable</param>
        /// <param name="text">The text in the button</param>
        /// <param name="tooltip">A tooltip for the button</param>
        /// <param name="texture">A texture for the button</param>
        /// <returns>True if the button was clicked, false otherwise.</returns>
        public static bool ConditionalButton(bool condition, string text, string tooltip = null, Texture texture = null)
        {
            return ConditionalButton(() => condition, text, tooltip, texture);
        }

        /// <summary>
        /// Creates a button that only becomes enabled if <paramref name="condition"/> is true
        /// </summary>
        /// <param name="condition">The condition for the button to be clickable</param>
        /// <param name="label">The label for the button</param>
        /// <returns>True if the button was clicked, false otherwise.</returns>
        public static bool ConditionalButton(bool condition, GUIContent label)
        {
            return ConditionalButton(() => condition, label);
        }

        /// <summary>
        /// Creates a button that only becomes enabled if <paramref name="condition"/> returns true
        /// </summary>
        /// <param name="condition">The condition for the button to be clickable</param>
        /// <param name="text">The text in the button</param>
        /// <param name="tooltip">A tooltip for the button</param>
        /// <param name="texture">A texture for the button</param>
        /// <returns>True if the button was clicked, false otherwise.</returns>
        public static bool ConditionalButton(Func<bool> condition, string text, string tooltip = null, Texture texture = null)
        {
            return ConditionalButton(condition, new GUIContent(text, texture, tooltip));
        }

        /// <summary>
        /// Creates a button that only becomes enabled if <paramref name="condition"/> returns true
        /// </summary>
        /// <param name="condition">The condition for the button to be clickable</param>
        /// <param name="label">The label for the button</param>
        /// <returns>True if the button was clicked, false otherwise.</returns>
        public static bool ConditionalButton(Func<bool> condition, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(!condition());
            bool buttonVal = GUILayout.Button(label);
            EditorGUI.EndDisabledGroup();
            return buttonVal;
        }

        /// <summary>
        /// Checks if the type specified in <paramref name="type"/> can be drawn using IMGUI
        /// </summary>
        /// <returns>True if the type can be drawn, false otherwise</returns>
        public static bool CanDrawFieldFromType(Type type)
        {
            return typeDrawers.ContainsKey(type) || type.IsEnum;
        }

        /// <summary>
        /// Checks if the FieldInfo specifieed in <paramref name="fieldInfo"/> can be drawn using IMGUI
        /// </summary>
        /// <param name="fieldInfo">The field to draw</param>
        /// <returns>True if the type can be drawn, false otherwise</returns>
        public static bool CanDrawFieldFromFieldInfo(FieldInfo fieldInfo)
        {
            return CanDrawFieldFromType(fieldInfo.FieldType);
        }

        /// <summary>
        /// Creates a Mask field for the project's Layers
        /// </summary>
        /// <param name="mask">The current value of the LayerMask</param>
        /// <param name="label">The label for the field</param>
        /// <returns>The new modified mask</returns>
        public static LayerMask LayerMaskField(LayerMask mask, GUIContent label)
        {
            return EditorGUILayout.MaskField(label, UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(mask), UnityEditorInternal.InternalEditorUtility.layers);
        }

        /// <summary>
        /// Draws a field of type <paramref name="type"/>
        /// <br>Throws a <see cref="NotImplementedException"/> is the type specified in <paramref name="type"/> cannot be drawn</br>
        /// </summary>
        /// <param name="type">The type to draw</param>
        /// <param name="fieldValue">The value of the field</param>
        /// <param name="newValue">The new value of the field which was picked by the user</param>
        /// <param name="content">A label/tooltip for the field</param>
        /// <returns>True if the value has changed and newValue has indeed a new value, false otherwise.</returns>
        public static bool DrawFieldWithType(Type type, object fieldValue, out object newValue, GUIContent content)
        {
            if (typeDrawers.TryGetValue(type, out var drawer))
            {
                EditorGUI.BeginChangeCheck();
                newValue = drawer(content, fieldValue);
                return EditorGUI.EndChangeCheck();
            }
            else if (type.IsEnum)
            {
                EditorGUI.BeginChangeCheck();
                if (type.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    newValue = enumFlagsTypeHandler(content, fieldValue);
                }
                else
                {
                    newValue = enumTypeHandler(content, fieldValue);
                }
                return EditorGUI.EndChangeCheck();
            }
            throw new NotImplementedException($"Cannot draw a field of type {type.Name}");
        }

        /// <summary>
        /// Draws a field from the <see cref="FieldInfo"/> specified in <paramref name="fieldInfo"/>
        /// <br>Throws a <see cref="NotImplementedException"/> if the FieldType from the FieldInfo specified in <paramref name="fieldInfo"/> cannot be drawn</br>
        /// </summary>
        /// <param name="fieldInfo">The Field to draw</param>
        /// <param name="fieldValue">The value of the field</param>
        /// <param name="newValue">The new value of the field which was picked by the user</param>
        /// <param name="content">A label/tooltip for the field</param>
        /// <returns>True if the value has changed and newValue has indeed a new value, false otherwise.</returns>
        public static bool DrawFieldFromFieldInfo(FieldInfo fieldInfo, object fieldValue, out object newValue, GUIContent guiContent = null)
        {
            guiContent = guiContent ?? new GUIContent
            {
                text = ObjectNames.NicifyVariableName(fieldInfo.Name),
                tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip
            };

            Type fieldType = fieldInfo.FieldType;
            return DrawFieldWithType(fieldType, fieldValue, out newValue, guiContent);
        }

        static IMGUIUtil()
        {
            typeDrawers.Add(typeof(bool), (labelTooltip, value) => EditorGUILayout.Toggle(labelTooltip, (bool)value));
            typeDrawers.Add(typeof(long), (labelTooltip, value) => EditorGUILayout.LongField(labelTooltip, (long)value));
            typeDrawers.Add(typeof(int), (labelTooltip, value) => EditorGUILayout.IntField(labelTooltip, (int)value));
            typeDrawers.Add(typeof(float), (labelTooltip, value) => EditorGUILayout.FloatField(labelTooltip, (float)value));
            typeDrawers.Add(typeof(double), (labelTooltip, value) => EditorGUILayout.DoubleField(labelTooltip, (double)value));
            typeDrawers.Add(typeof(string), (labelTooltip, value) => EditorGUILayout.TextField(labelTooltip, (string)value));
            typeDrawers.Add(typeof(Vector2), (labelTooltip, value) => EditorGUILayout.Vector2Field(labelTooltip, (Vector2)value));
            typeDrawers.Add(typeof(Vector3), (labelTooltip, value) => EditorGUILayout.Vector3Field(labelTooltip, (Vector3)value));
            typeDrawers.Add(typeof(Color), (labelTooltip, value) => EditorGUILayout.ColorField(labelTooltip, (Color)value));

            typeDrawers.Add(typeof(Color32), (labelTooltip, value) =>
            {
                return (Color32)EditorGUILayout.ColorField(labelTooltip, (Color32)value);
            });

            typeDrawers.Add(typeof(AnimationCurve), (labelTooltip, value) => EditorGUILayout.CurveField(labelTooltip, (AnimationCurve)value ?? new AnimationCurve()));
            typeDrawers.Add(typeof(LayerMask), (labelTooltip, value) => LayerMaskField((LayerMask)value, labelTooltip));
            typeDrawers.Add(typeof(Vector4), (labelTooltip, value) => EditorGUILayout.Vector4Field(labelTooltip, (Vector4)value));
            typeDrawers.Add(typeof(Rect), (labelTooltip, value) => EditorGUILayout.RectField(labelTooltip, (Rect)value));
            typeDrawers.Add(typeof(RectInt), (labelTooltip, value) => EditorGUILayout.RectIntField(labelTooltip, (RectInt)value));
            typeDrawers.Add(typeof(char), (labelTooltip, value) =>
            {
                string val = ((char)value).ToString();
                val = EditorGUILayout.TextField(labelTooltip, val);
                return val.ToCharArray().FirstOrDefault();
            });
            typeDrawers.Add(typeof(Bounds), (labelTooltip, value) => EditorGUILayout.BoundsField(labelTooltip, (Bounds)value));
            typeDrawers.Add(typeof(BoundsInt), (labelTooltip, value) => EditorGUILayout.BoundsIntField(labelTooltip, (BoundsInt)value));
            typeDrawers.Add(typeof(Quaternion), (labelTooltip, value) =>
            {
                Quaternion quat = (Quaternion)value;
                Vector3 euler = quat.eulerAngles;
                euler = EditorGUILayout.Vector3Field(labelTooltip, euler);
                return Quaternion.Euler(euler);
            });
            typeDrawers.Add(typeof(Vector2Int), (labelTooltip, value) => EditorGUILayout.Vector2IntField(labelTooltip, (Vector2Int)value));
            typeDrawers.Add(typeof(Vector3Int), (labelTooltip, value) => EditorGUILayout.Vector3IntField(labelTooltip, (Vector3Int)value));
            enumFlagsTypeHandler = (labelTooltip, value) => EditorGUILayout.EnumFlagsField(labelTooltip, (Enum)value);
            enumTypeHandler = (labelTooltip, value) => EditorGUILayout.EnumPopup(labelTooltip, (Enum)value);
        }
    }
}
