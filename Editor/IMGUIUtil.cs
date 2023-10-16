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
    public static class IMGUIUtil
    {
        private delegate object FieldDrawHandler(GUIContent labelTooltip, object value);
        private static readonly Dictionary<Type, FieldDrawHandler> typeDrawers = new Dictionary<Type, FieldDrawHandler>();

#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
        private static FieldDrawHandler enumFlagsTypeHandler;
        private static FieldDrawHandler enumTypeHandler;
#endif

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

        public static GUIContent GetLabelFromProperty(SerializedProperty prop)
        {
            return new GUIContent(prop.displayName, prop.tooltip);
        }

        public static bool ConditionalButton(bool condition, string text, string tooltip = null, Texture texture = null)
        {
            return ConditionalButton(() => condition, text, tooltip, texture);
        }

        public static bool ConditionalButton(bool condition, GUIContent label)
        {
            return ConditionalButton(() => condition, label);
        }

        public static bool ConditionalButton(Func<bool> condition, string text, string tooltip = null, Texture texture = null)
        {
            return ConditionalButton(condition, new GUIContent(text, texture, tooltip));
        }

        public static bool ConditionalButton(Func<bool> condition, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(!condition());
            bool buttonVal = GUILayout.Button(label);
            EditorGUI.EndDisabledGroup();
            return buttonVal;
        }

        public static bool ConditionalButtonAction(Action action, bool condition, string text, string tooltip = null, Texture texture = null)
        {
            return ConditionalButtonAction(action, () => condition, text, tooltip, texture);
        }
        public static bool ConditionalButtonAction(Action action, bool condition, GUIContent content)
        {
            return ConditionalButtonAction(action, () => condition, content);
        }

        public static bool ConditionalButtonAction(Action action, Func<bool> condition, string text, string tooltip = null, Texture texture = null)
        {
            return ConditionalButtonAction(action, condition, new GUIContent(text, texture, tooltip));
        }

        public static bool ConditionalButtonAction(Action action, Func<bool> condition, GUIContent content)
        {
            var val = ConditionalButton(condition, content);
            if (val)
                action();
            return val;
        }
        public static bool ButtonAction(Action action, string text, string tooltip = null, Texture texture = null, params GUILayoutOption[] options) => ButtonAction(action, new GUIContent(text, texture, tooltip), options);

        public static bool ButtonAction(Action action, GUIContent label, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(label, options))
            {
                action();
                return true;
            }
            return false;
        }

        public static bool CanDrawFieldFromType(Type type)
        {
            return typeDrawers.ContainsKey(type) || type.IsEnum;
        }

        public static bool CanDrawFieldFromFieldInfo(FieldInfo fieldInfo)
        {
            return typeDrawers.ContainsKey(fieldInfo.FieldType) || fieldInfo.FieldType.IsEnum;
        }


        public static LayerMask LayerMaskField(LayerMask mask, GUIContent label)
        {
            return EditorGUILayout.MaskField(label, UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(mask), UnityEditorInternal.InternalEditorUtility.layers);
        }

        public static bool DrawFieldWithType(Type type, object fieldValue, out object newValue, GUIContent content)
        {
#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
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
#else
            if (typeDrawers.TryGetValue(type, out var handler))
            {
                EditorGUI.BeginChangeCheck();
                newValue = handler(content, fieldValue);
                return EditorGUI.EndChangeCheck();
            }
#endif
            throw new NotImplementedException($"Cannot draw a field of type {type.Name}");
        }
        public static bool DrawFieldFromFieldInfo(FieldInfo fieldInfo, object fieldValue, out object newValue, GUIContent guiContent = null)
        {
            guiContent = guiContent ?? new GUIContent
            {
                text = ObjectNames.NicifyVariableName(fieldInfo.Name),
                tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip
            };

            Type fieldType = fieldInfo.FieldType;

#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
            if(typeDrawers.TryGetValue(fieldType, out var drawer))
            {
                EditorGUI.BeginChangeCheck();
                newValue = drawer(guiContent, fieldValue);
                return EditorGUI.EndChangeCheck();
            }
            else if(fieldType.IsEnum)
            {
                EditorGUI.BeginChangeCheck();
                if(fieldType.GetCustomAttribute<FlagsAttribute>() != null || fieldInfo.GetCustomAttribute<EnumMaskAttribute>() != null)
                {
                    newValue = enumFlagsTypeHandler(guiContent, fieldValue);
                }
                else
                {
                    newValue = enumTypeHandler(guiContent, fieldValue);
                }
                return EditorGUI.EndChangeCheck();
            }
#else
            if (typeDrawers.TryGetValue(fieldType, out var handler))
            {
                EditorGUI.BeginChangeCheck();
                newValue = handler(guiContent, fieldValue);
                return EditorGUI.EndChangeCheck();
            }
#endif
            throw new NotImplementedException($"Cannot draw a field of type {fieldType.Name}");
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

#if BBEPIS_BEPINEXPACK || RISKOFTHUNDER_ROR2BEPINEXPACK
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
#endif
        }
    }
}
