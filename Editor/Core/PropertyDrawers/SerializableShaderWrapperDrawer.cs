using RoR2EditorKit.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableShaderWrapper))]
    public sealed class SerializableShaderWrapperDrawer : PropertyDrawer
    {
        Object shaderObj = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
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
            if(EditorGUI.EndChangeCheck())
            {
                shaderNameProp.stringValue = shaderObj == null ? string.Empty : ((Shader)shaderObj).name;
                shaderGUIDProp.stringValue = shaderObj == null ? string.Empty : AssetDatabaseUtils.GetGUIDFromAsset(shaderObj);
            }
            EditorGUI.EndProperty();
        }

    }
}