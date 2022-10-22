﻿using RoR2EditorKit.Settings;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(MaterialEditorSettings.ShaderStringPair))]
    public sealed class ShaderStringPairDrawer : PropertyDrawer
    {
        Object shaderObj = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shaderRefProp = property.FindPropertyRelative("shader");
            var shaderNameProp = shaderRefProp.FindPropertyRelative("shaderName");
            var shaderGUIDProp = shaderRefProp.FindPropertyRelative("shaderGUID");

            shaderObj = Shader.Find(shaderNameProp.stringValue);
            if(!shaderObj)
            {
                shaderObj = AssetDatabaseUtils.LoadAssetFromGUID<Object>(shaderGUIDProp.stringValue);
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            shaderObj = EditorGUI.ObjectField(position, ObjectNames.NicifyVariableName(property.FindPropertyRelative("shaderName").stringValue), shaderObj, typeof(Shader), false);
            if(EditorGUI.EndChangeCheck())
            {

                shaderNameProp.stringValue = shaderObj == null ? string.Empty : ((Shader)shaderObj).name;
                shaderGUIDProp.stringValue = shaderObj == null ? string.Empty : AssetDatabaseUtils.GetGUIDFromAsset(shaderObj);
            }
        }
    }
    //[CustomPropertyDrawer(typeof(ShaderStringPair))]
    /*public sealed class ShaderStringPairPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var objRefProperty = property.FindPropertyRelative("shader");
            objRefProperty.objectReferenceValue = EditorGUI.ObjectField(position, ObjectNames.NicifyVariableName(property.FindPropertyRelative("shaderName").stringValue), objRefProperty.objectReferenceValue, typeof(Shader), false);
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16;
        }
    }*/
}
