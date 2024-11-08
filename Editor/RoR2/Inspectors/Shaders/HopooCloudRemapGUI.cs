using System;
using UnityEditor;
using UnityEngine.Rendering;

//I hate that this exists, but we cant add easily an inspector to a material, and ror2ek's system is fucking awful honestly. To the shadowrealm it goes.
namespace RoR2 //.Editor //Do not uncomment the editor, i just left it there so i dont feel too bad about it.
{
    public class HopooCloudRemapGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            foreach (var prop in properties)
            {
                switch (prop.name)
                {
                    case "_SrcBlend":
                    case "_DstBlend":
                    case "_InternalSimpleBlendMode":
                        DrawBlendEnum(materialEditor, prop);
                        break;
                    default:
                        materialEditor.ShaderProperty(prop, prop.displayName);
                        break;
                }
            }
        }

        private void DrawBlendEnum(MaterialEditor editor, MaterialProperty property)
        {
            EditorGUI.BeginChangeCheck();
            property.floatValue = Convert.ToSingle(EditorGUILayout.EnumPopup(property.displayName, (BlendMode)property.floatValue));
            if (EditorGUI.EndChangeCheck())
            {
                editor.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}