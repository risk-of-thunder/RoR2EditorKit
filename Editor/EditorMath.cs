using HG.BlendableTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoR2EditorKit
{
    public static class EditorMath
    {
        private static MethodInfo _gridSizeGetMethod;
        public static Vector3 RoundToNearestGrid(Vector3 position, Vector3? gridSize = null)
        {
            gridSize = gridSize ?? (Vector3)_gridSizeGetMethod.Invoke(null, null);

            var x = RoundToNearestGridValue(position.x, gridSize.Value.x);
            var y = RoundToNearestGridValue(position.y, gridSize.Value.y);
            var z = RoundToNearestGridValue(position.z, gridSize.Value.z);

            return new Vector3(x, y, z);
        }

        public static float RoundToNearestGridValue(float pos, float gridValue)
        {
            float xDiff = pos % gridValue;
            bool isPositive = pos > 0 ? true : false;
            pos -= xDiff;
            if (Mathf.Abs(xDiff) > (gridValue / 2))
            {
                if (isPositive)
                {
                    pos += gridValue;
                }
                else
                {
                    pos -= gridValue;
                }
            }
            return pos;
        }

        public static float GetAverage(Vector3 vector)
        {
            return (vector.x + vector.y + vector.z) / 3;
        }

        public static float GetAverage(Vector2 vector)
        {
            return (vector.x + vector.y) / 2;
        }
        public static Vector3 MultiplyElementWise(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector2 MultiplyElementWise(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public static Vector3 DivideElementWise(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vector2 DivideElementWise(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        public static Vector3 Floor(Vector3 vector)
        {
            return new Vector3(Mathf.Floor(vector.x), Mathf.Floor(vector.y), Mathf.Floor(vector.z));
        }

        public static Vector2 Floor(Vector2 vector)
        {
            return new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
        }

        public static Vector3 Ceil(Vector3 vector)
        {
            return new Vector3(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y), Mathf.Ceil(vector.z));
        }

        public static Vector2 Ceil(Vector2 vector)
        {
            return new Vector2(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y));
        }

        public static Vector3 Round(Vector3 vector)
        {
            return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
        }

        public static Vector2 Round(Vector2 vector)
        {
            return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
        }

        static EditorMath()
        {
            _gridSizeGetMethod = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.GridSettings").GetProperty("size").GetGetMethod();
        }
    }
}