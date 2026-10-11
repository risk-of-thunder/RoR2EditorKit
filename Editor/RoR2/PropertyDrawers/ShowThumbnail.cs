using HG;
using RoR2.Orbs;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShowThumbnailAttribute))]
    public class ShowThumbnailPropertyDrawer : IMGUIPropertyDrawer<ShowThumbnailAttribute>
    {
        public float previewSize => GetOrCreateSetting("previewSize", 64f, EditorSettingManager.SettingType.UserSetting);

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.yMax -= previewSize;
            EditorGUI.PropertyField(position, property, label);
            position.yMax += previewSize;
            position.yMin += 16;

            position.xMax = position.xMin + previewSize;

            Either<Texture, Sprite> textureOrSprite = default;
            if(property.objectReferenceValue is Texture t)
            {
                textureOrSprite = t;
            }
            else if(property.objectReferenceValue is Sprite s)
            {
                textureOrSprite = s;
            }

            if(textureOrSprite.a || textureOrSprite.b)
            {
                DrawTextureOrSprite(textureOrSprite, position);
            }
            else
            {
                EditorGUI.DrawRect(position, Color.black);
            }
        }

        private void DrawTextureOrSprite(Either<Texture, Sprite> textureOrSprite, Rect position)
        {
            if(textureOrSprite.a)
            {
                Texture texture = textureOrSprite.a;
                EditorGUI.DrawPreviewTexture(position, texture);
            }
            else if(textureOrSprite.b)
            {
                Sprite sprite = textureOrSprite.b;
                Rect pixelRect = sprite.rect;
                Rect normalizedTexCoords = pixelRect;
                normalizedTexCoords.x /= sprite.texture.width;
                normalizedTexCoords.width /= sprite.texture.width;
                normalizedTexCoords.y /= sprite.texture.height;
                normalizedTexCoords.height /= sprite.texture.height;
                GUI.DrawTextureWithTexCoords(position, sprite.texture, normalizedTexCoords);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + previewSize;
        }
    }
}