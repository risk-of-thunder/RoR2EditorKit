using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableShaderWrapper))]
    internal sealed class SerializableShaderWrapperDrawer : IMGUIPropertyDrawer<SerializableShaderWrapper>
    {
        Object shaderObj = null;
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shaderNameProp = property.FindPropertyRelative("shaderName");
            var shaderGUIDProp = property.FindPropertyRelative("shaderGUID");

            shaderObj = Shader.Find(shaderNameProp.stringValue);
            if (!shaderObj)
            {
                shaderObj = AssetDatabaseUtils.LoadAssetFromGUID<Object>(shaderGUIDProp.stringValue);
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            shaderObj = EditorGUI.ObjectField(position, label, shaderObj, typeof(Shader), false);
            if (EditorGUI.EndChangeCheck())
            {
                shaderNameProp.stringValue = shaderObj == null ? string.Empty : ((Shader)shaderObj).name;
                shaderGUIDProp.stringValue = shaderObj == null ? string.Empty : AssetDatabaseUtils.GetGUIDFromAsset(shaderObj);
            }
            EditorGUI.EndProperty();
        }
    }
}