using System;
using UnityEngine;

namespace RoR2.Editor.GameMaterialSystem
{
    [Serializable]
    public struct TextureReference
    {
        public string guid;
        public Vector2 offset;
        public Vector2 scale;
    }
}