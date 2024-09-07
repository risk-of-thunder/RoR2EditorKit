using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableShaderWrapper))]
    internal class SerializableShaderWrapperDrawer : IMGUIPropertyDrawer<SerializableShaderWrapper>
    {
        Object shaderObj = null;
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shaderNameProp = property.FindPropertyRelative(nameof(SerializableShaderWrapper._shaderName));
            var shaderGUIDProp = property.FindPropertyRelative(nameof(SerializableShaderWrapper._shaderGuid));

            shaderObj = Shader.Find(shaderNameProp.stringValue);
            if (!shaderObj)
            {
                shaderObj = AssetDatabaseUtil.LoadAssetFromGUID<Object>(shaderGUIDProp.stringValue);
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            shaderObj = EditorGUI.ObjectField(position, label, shaderObj, typeof(Shader), false);
            if (EditorGUI.EndChangeCheck())
            {
                shaderNameProp.stringValue = shaderObj == null ? string.Empty : ((Shader)shaderObj).name;
                shaderGUIDProp.stringValue = shaderObj == null ? string.Empty : AssetDatabaseUtil.GetAssetGUIDString(shaderObj);
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndProperty();
        }
    }
}