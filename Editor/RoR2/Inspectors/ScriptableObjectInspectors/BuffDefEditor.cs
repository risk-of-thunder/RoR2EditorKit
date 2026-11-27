using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(BuffDef))]
    public class BuffDefEditor : ScriptableObjectIMGUIEditor<BuffDef>
    {
        private Material GetSpriteMaterial()
        {
            if(!_spriteMaterial)
            {
                _spriteMaterial = new Material(Shader.Find("Sprites/Default"));
                _spriteMaterial.SetColor(GetColorID(), serializedObject.FindProperty("buffColor").colorValue);
            }
            return _spriteMaterial;
        }
        private Material _spriteMaterial;
        private int GetColorID()
        {
            return _colorId ??= Shader.PropertyToID("_Color");
        }
        private int? _colorId;
        protected override void DrawIMGUI()
        {
            SerializedProperty iterator = serializedObject.GetIterator();

            bool visitChildren = true;
            while (iterator.NextVisible(visitChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    if(iterator.propertyPath == "buffColor")
                    {
                        DrawColoredSprite(iterator);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                visitChildren = false;
            }
        }

        private void DrawColoredSprite(SerializedProperty buffColorProperty)
        {
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(buffColorProperty, true);
                if (changeCheck.changed)
                {
                    Material spriteMat = GetSpriteMaterial();
                    spriteMat.SetColor(GetColorID(), buffColorProperty.colorValue);
                }
            }
            var sprite = serializedObject.FindProperty("iconSprite").objectReferenceValue as Sprite;
            var rect = GUILayoutUtility.GetRect(128, 128 + EditorGUIUtility.singleLineHeight); //Buff icons by standard are 128x128
            rect.width = 128;
            rect.height = 128;
            rect.y += EditorGUIUtility.singleLineHeight / 2;
            rect.x = Screen.width / 2;
            rect.x -= 64;

            if (sprite)
            {
                EditorGUI.DrawPreviewTexture(rect, sprite.texture, GetSpriteMaterial(), ScaleMode.ScaleToFit, 0, -1);
            }
            else
            {
                EditorGUI.DrawRect(rect, Color.black);
            }
        }
    }
}