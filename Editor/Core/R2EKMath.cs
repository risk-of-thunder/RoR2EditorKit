using System.Reflection;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// Contains math related utilities
    /// </summary>
    public static class R2EKMath
    {
        private static MethodInfo _gridSizeGetMethod;

        /// <summary>
        /// Rounds the given Vector3 to the nearest grid position.
        /// </summary>
        /// <param name="position">The position to round</param>
        /// <param name="gridSize">The grid's size, if no grid is provided, the Editor's grid is used</param>
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
        /// Rounds the given <paramref name="pos"/> to the nearest <paramref name="gridValue"/>
        /// </summary>
        /// <param name="pos">The number to round</param>
        /// <param name="gridValue">The grid's value</param>
        /// <returns><paramref name="pos"/>, rounded to the nearest grid value</returns>
        public static float RoundToNearestGridValue(float pos, float gridValue)
        {
            float diff = pos % gridValue;
            bool isPositive = pos > 0 ? true : false;
            pos -= diff;
            if (Mathf.Abs(diff) > (gridValue / 2))
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
        /// Returns the average of <paramref name="vector"/> by adding all the components of it and dividing it by 3
        /// </summary>
        /// <param name="vector">The vector to get it's average</param>
        /// <returns>The average between all 3 components of the vector</returns>
        public static float GetAverage(Vector3 vector)
        {
            return (vector.x + vector.y + vector.z) / 3;
        }


        /// <summary>
        /// Returns the average of <paramref name="vector"/> by adding all the components of it and dividing it by 2
        /// </summary>
        /// <param name="vector">The vector to get it's average</param>
        /// <returns>The average between all 2 components of the vector</returns>
        public static float GetAverage(Vector2 vector)
        {
            return (vector.x + vector.y) / 2;
        }

        /// <summary>
        /// Multiplies two vectors by multiplying each component with the other component
        /// </summary>
        /// <param name="lhs">The left vector of the  multiplication</param>
        /// <param name="rhs">The right vector of the multiplication</param>
        /// <returns>The new vector</returns>
        public static Vector3 MultiplyElementWise(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        }

        /// <summary>
        /// Multiplies two vectors by multiplying each component with the other component
        /// </summary>
        /// <param name="lhs">The left vector of the  multiplication</param>
        /// <param name="rhs">The right vector of the multiplication</param>
        /// <returns>The new vector</returns>
        public static Vector2 MultiplyElementWise(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// Divides the <paramref name="lhs"/>'s components by <paramref name="rhs"/>'s components
        /// </summary>
        /// <param name="lhs">The dividend for the operation</param>
        /// <param name="rhs">The divisor for the operation</param>
        /// <returns>The resulting vector</returns>
        public static Vector3 DivideElementWise(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
        }


        /// <summary>
        /// Divides the <paramref name="lhs"/>'s components by <paramref name="rhs"/>'s components
        /// </summary>
        /// <param name="lhs">The dividend for the operation</param>
        /// <param name="rhs">The divisor for the operation</param>
        /// <returns>The resulting vector</returns>
        public static Vector2 DivideElementWise(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        /// <summary>
        /// Applies <see cref="Mathf.Floor(float)"/> to each of the vector's components
        /// </summary>
        /// <param name="vector">The vector to floor</param>
        /// <returns>The floored vector</returns>
        public static Vector3 Floor(Vector3 vector)
        {
            return new Vector3(Mathf.Floor(vector.x), Mathf.Floor(vector.y), Mathf.Floor(vector.z));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Floor(float)"/> to each of the vector's components
        /// </summary>
        /// <param name="vector">The vector to floor</param>
        /// <returns>The floored vector</returns>
        public static Vector2 Floor(Vector2 vector)
        {
            return new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Ceil(float)"/> to each of the vector's components
        /// </summary>
        /// <param name="vector">The vector to Ceil</param>
        /// <returns>The Ceiled vector</returns>
        public static Vector3 Ceil(Vector3 vector)
        {
            return new Vector3(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y), Mathf.Ceil(vector.z));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Ceil(float)"/> to each of the vector's components
        /// </summary>
        /// <param name="vector">The vector to Ceil</param>
        /// <returns>The Ceiled vector</returns>
        public static Vector2 Ceil(Vector2 vector)
        {
            return new Vector2(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Round(float)"/> to each of the vector's components
        /// </summary>
        /// <param name="vector">The vector to Round</param>
        /// <returns>The Rounded vector</returns>
        public static Vector3 Round(Vector3 vector)
        {
            return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
        }

        /// <summary>
        /// Applies <see cref="Mathf.Round(float)"/> to each of the vector's components
        /// </summary>
        /// <param name="vector">The vector to Round</param>
        /// <returns>The Rounded vector</returns>
        public static Vector2 Round(Vector2 vector)
        {
            return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
        }

        /// <summary>
        /// A direct rip of the base game's Remap method.
        /// </summary>
        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return outMin + (value - inMin) / (inMax - inMin) * (outMax - outMin);
        }

        static R2EKMath()
        {
            _gridSizeGetMethod = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.GridSettings").GetProperty("size").GetGetMethod();
        }
    }
}