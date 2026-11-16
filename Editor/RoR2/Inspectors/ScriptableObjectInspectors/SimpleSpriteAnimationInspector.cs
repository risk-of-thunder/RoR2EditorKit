using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(SimpleSpriteAnimation))]
    public class SimpleSpriteAnimationInspector : ScriptableObjectIMGUIEditor<SimpleSpriteAnimation>
    {
        Texture2D spriteSource;
        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Sprite Filling Utility", EditorStyles.boldLabel);
            using var _ = new EditorGUILayout.VerticalScope("Box");

            spriteSource = (Texture2D)EditorGUILayout.ObjectField("Wawa", spriteSource, typeof(Texture2D), false);
            if(GUILayout.Button("Fill array with sprite data") && spriteSource)
            {
                Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(spriteSource)).OfType<Sprite>().ToArray();

                Undo.RegisterCompleteObjectUndo(serializedObject.targetObject, "Fill array with sprite data");
                SerializedProperty framesProperty = serializedObject.FindProperty("frames");
                framesProperty.arraySize = sprites.Length;
                for(int i = 0; i < sprites.Length; i++)
                {
                    var frameProperty = framesProperty.GetArrayElementAtIndex(i);

                    var spriteProperty = frameProperty.FindPropertyRelative("sprite");
                    var durationProperty = frameProperty.FindPropertyRelative("duration");

                    spriteProperty.objectReferenceValue = sprites[i];
                    durationProperty.intValue = 1;
                }
            }
        }
    }
}