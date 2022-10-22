using System;
using UnityEditor;
using UnityEngine;
using static RoR2EditorKit.Core.Inspectors.ExtendedMaterialInspector;
using BlendMode = UnityEngine.Rendering.BlendMode;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [InitializeOnLoad]
    internal static class HGFXInspectors
    {
        static HGFXInspectors()
        {
            if (MaterialEditorEnabled)
                AddShaderEditor("hgCloudRemap", HGCloudRemapEditor, typeof(HGFXInspectors));
        }

        public static void HGCloudRemapEditor()
        {
            DrawBlendEnumProperty(GetProperty("_SrcBlend"));
            DrawBlendEnumProperty(GetProperty("_DstBlend"));
            var prop = GetProperty("_InternalSimpleBlendMode");
            DrawBlendEnumProperty(GetProperty("_InternalSimpleBlendMode"), new GUIContent(prop.displayName, "If the BlendOp command is used, the blending operation is set to that value. Otherwise, the blending operation defaults to Add."));
            DrawProperty("_TintColor");
            DrawProperty("_DisableRemapOn");
            DrawProperty("_MainTex");
            DrawProperty("_RemapTex");
            DrawProperty("_InvFade");
            DrawProperty("_Boost");
            DrawProperty("_AlphaBoost");
            DrawProperty("_AlphaBias");
            DrawProperty("_UseUV1On");
            DrawProperty("_FadeCloseOn");
            DrawProperty("_FadeCloseDistance");
            DrawProperty("_Cull");
            DrawProperty("_ZTest");
            DrawProperty("_DepthOffset");
            DrawProperty("_CloudsOn");
            DrawProperty("_CloudOffsetOn");
            DrawProperty("_DistortionStrength");
            DrawProperty("_Cloud1Tex");
            DrawProperty("_Cloud2Tex");
            DrawProperty("_CutoffScroll");
            DrawProperty("_VertexColorOn");
            DrawProperty("_VertexAlphaOn");
            DrawProperty("_CalcTextureAlphaOn");
            DrawProperty("_VertexOffsetOn");
            DrawProperty("_FresnelOn");
            DrawProperty("_SkyboxOnly");
            DrawProperty("_FresnelPower");
            DrawProperty("_OffsetAmount");
        }

        private static void DrawBlendEnumProperty(MaterialProperty prop, GUIContent guiContent = null)
        {
            if(guiContent == null)
            {
                guiContent = new GUIContent(prop.displayName);
            }
            prop.floatValue = Convert.ToSingle(EditorGUILayout.EnumPopup(guiContent, (BlendMode)prop.floatValue));
        }
    }
}