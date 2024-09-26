using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace RoR2.Editor.GameMaterialSystem
{
    [CustomEditor(typeof(Material))]
    public class SerializedMaterialDataEditor : MaterialEditor
    {
        protected override void OnHeaderGUI()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var importer = AssetImporter.GetAtPath(assetPath);
            var material = target as Material;
            if(importer is not SerializableMaterialDataImporter smdi)
            {
                base.OnHeaderGUI();
                return;
            }

            GUI.Box(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 46), new GUIContent(), BaseStyles.inspectorBig);
            var cursor = EditorGUILayout.GetControlRect();
            cursor = new Rect(cursor.x, cursor.y + 4, cursor.width, cursor.height);
            OnPreviewGUI(new Rect(cursor.x + 2, cursor.y + 2, 32, 32), BaseStyles.inspectorBigInner);

            cursor = new Rect(cursor.x + 40, cursor.y, cursor.width, cursor.height);
            GUI.Label(cursor, target.name, EditorStyles.largeLabel);


            var shaderLabelContent = new GUIContent("Shader");
            var offset = EditorStyles.label.CalcSize(shaderLabelContent).x + 10;
            cursor = new Rect(cursor.x + 2, cursor.y + 22, offset - 2, cursor.height);
            GUI.Label(cursor, shaderLabelContent, EditorStyles.label);

            cursor = new Rect(cursor.x + offset + 7, cursor.y - 1, EditorGUIUtility.currentViewWidth - (cursor.width + cursor.x) - 16, cursor.height);

            if(EditorGUI.DropdownButton(cursor, new GUIContent(material.shader.name), FocusType.Passive))
            {
                var popup = new StubbedShaderDropdown(null);
                popup.onShaderSelected += OnSelectedShaderPopup;
                popup.Show(cursor);
            }


            cursor = new Rect(cursor.x - offset - 50, cursor.y + 5, 32, cursor.height);


            GUILayout.Space(32);

            void OnSelectedShaderPopup(StubbedShaderDropdown.Item item)
            {
                var serializedObject = new SerializedObject(smdi);
                serializedObject.Update();
                SerializableShaderWrapper shaderWrapper = item.shader;
                if(shaderWrapper != null)
                {
                    Shader shader = shaderWrapper.shader;
                    if(shader)
                    {
                        var shaderProp = serializedObject.FindProperty("shader");
                        var shaderNameProp = shaderProp.FindPropertyRelative("_shaderName");
                        var shaderGUIDProp = shaderProp.FindPropertyRelative("_shaderGuid");

                        shaderNameProp.stringValue = shader.name;
                        shaderGUIDProp.stringValue = AssetDatabaseUtil.GetAssetGUIDString(shader);

                        material.shader = shader;
                        WriteChanges();
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var smdi = AssetImporter.GetAtPath(assetPath) as SerializableMaterialDataImporter;
            if (smdi)
            {
                target.hideFlags = HideFlags.None;
                EditorGUI.BeginChangeCheck();
            }
            base.OnInspectorGUI();
            if (smdi && EditorGUI.EndChangeCheck())
            {
                WriteChanges();
            }
        }

        private void WriteChanges()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var material = target as Material;
            var smd = JsonUtility.FromJson<SerializableMaterialData>(File.ReadAllText(assetPath));
            var shaderData = SerializableMaterialData.Build(material, smd.identity);
            var jsonData = JsonUtility.ToJson(shaderData, true);
            File.WriteAllText(assetPath, jsonData);
        }
        private static class BaseStyles
        {
            public static readonly GUIContent open;

            public static readonly GUIStyle inspectorBig;

            public static readonly GUIStyle inspectorBigInner;

            public static readonly GUIStyle centerStyle;

            public static readonly GUIStyle postLargeHeaderBackground;

            static BaseStyles()
            {
                open = EditorGUIUtility.TrTextContent("Open");
                inspectorBig = new GUIStyle(GetStyle("In BigTitle"));
                inspectorBigInner = "IN BigTitle inner";
                postLargeHeaderBackground = "IN BigTitle Post";
                centerStyle = new GUIStyle();
                centerStyle.alignment = TextAnchor.MiddleCenter;
            }
            static GUIStyle GetStyle(string styleName)
            {
                GUIStyle gUIStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
                if (gUIStyle == null)
                {
                    Debug.LogError("Missing built-in guistyle " + styleName);
                    gUIStyle = new GUIStyle();
                    gUIStyle.name = "StyleNotFoundError";
                }

                return gUIStyle;
            }
        }
    }
}