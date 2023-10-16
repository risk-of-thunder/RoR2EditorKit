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
    /// <summary>
    /// Class containing math related methods
    /// </summary>
    public static class EditorMath
    {
        private static MethodInfo _gridSizeGetMethod;
        /// <summary>
        /// Rounds the given Vector3 in <paramref name="position"/> to the nearest value specified in <paramref name="gridSize"/>
        /// <br>If no value is given to <paramref name="gridSize"/>, the default unity grid is used.</br>
        /// </summary>
        /// <param name="position">The position to round</param>
        /// <param name="gridSize">The grid's size, defaults to the default unity grid</param>
        /// <returns>The rounded Vector3</returns>
        public static Vector3 RoundToNearestGrid(Vector3 position, Vector3? gridSize = null)
        {
            gridSize = gridSize ?? (Vector3)_gridSizeGetMethod.Invoke(null, null);

            var x = RoundToNearestGridValue(position.x, gridSize.Value.x);
            var y = RoundToNearestGridValue(position.y, gridSize.Value.y);
            var z = RoundToNearestGridValue(position.z, gridSize.Value.z);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Rounds a floating point number to a specified grid value
        /// </summary>
        /// <returns>the roundedd float</returns>
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


        /// <summary>
        /// Returns the average value of the 3 components of a Vector3
        /// </summary>
        /// <returns>The sum of all 3 components, divided by 3</returns>
        public static float GetAverage(Vector3 vector)
        {
            return (vector.x + vector.y + vector.z) / 3;
        }

        /// <summary>
        /// Returns the average value of the 2 components of a Vector2
        /// </summary>
        /// <returns>The sum of all 2 components, divided by 2</returns>
        public static float GetAverage(Vector2 vector)
        {
            return (vector.x + vector.y) / 2;
        }

        /// <summary>
        /// Returns the average value of the 4 components of a Vector4
        /// </summary>
        /// <returns>The sum of all 4 components, divided by 4</returns>
        public static float GetAverage(Vector4 vector)
        {
            return (vector.x + vector.y + vector.z + vector.w) / 4;
        }

        /// <summary>
        /// Multiplies the two vectors by multiplying each element with each other.
        /// </summary>
        /// <returns>The result of multiplying a.x * b.x, a.y * b.y, a.z * b.z</returns>
        public static Vector3 MultiplyElementWise(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Multiplies the two vectors by multiplying each element with each other.
        /// </summary>
        /// <returns>The result of multiplying a.x * b.x, a.y * b.y</returns>
        public static Vector2 MultiplyElementWise(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// Divides the two vectors by dividing each element with each other
        /// </summary>
        /// <returns>The result of dividing a.x / b.x, a.y / b.y, a.z / b.z</returns>
        public static Vector3 DivideElementWise(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        /// <summary>
        /// Divides the two vectors by dividing each element with each other
        /// </summary>
        /// <returns>The result of dividing a.x / b.x, a.y / b.y</returns>
        public static Vector2 DivideElementWise(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        /// <summary>
        /// Applies <see cref="Mathf.Floor(float)"/> to each of the Vector's components
        /// </summary>
        /// <returns>The floored vector</returns>
        public static Vector3 Floor(Vector3 vector)
        {
            return new Vector3(Mathf.Floor(vector.x), Mathf.Floor(vector.y), Mathf.Floor(vector.z));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Floor(float)"/> to each of the Vector's components
        /// </summary>
        /// <returns>The floored vector</returns>
        public static Vector2 Floor(Vector2 vector)
        {
            return new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Ceil(float)"/> to each of the Vector's components
        /// </summary>
        /// <returns>The ceiled vector</returns>
        public static Vector3 Ceil(Vector3 vector)
        {
            return new Vector3(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y), Mathf.Ceil(vector.z));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Ceil(float)"/> to each of the Vector's components
        /// </summary>
        /// <returns>The ceiled vector</returns>
        public static Vector2 Ceil(Vector2 vector)
        {
            return new Vector2(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Round(float)"/> to each of the vector's components
        /// </summary>
        /// <returns>The rounded vector</returns>
        public static Vector3 Round(Vector3 vector)
        {
            return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Round(float)"/> to each of the vector's components
        /// </summary>
        /// <returns>The rounded vector</returns>
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