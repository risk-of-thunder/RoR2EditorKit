using System.Collections.Generic;

namespace RoR2.Editor
{
    /// <summary>
    /// Various extension methods from R2EK
    /// </summary>
    public static class R2EKExtensions
    {
        /// <summary>
        /// Calls <see cref="string.IsNullOrEmpty(string)"/> and <see cref="string.IsNullOrWhiteSpace(string)"/> on the given string and computes the logical OR value
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if the string is null, empty or whitespace, otherwise false</returns>
        public static bool IsNullOrEmptyOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Deconstruction utility for key value pairs
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}