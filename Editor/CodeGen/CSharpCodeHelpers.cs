using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoR2EditorKit.CodeGen
{
    /// <summary>
    /// Class containing utility methods for writing C# code using strings
    /// </summary>
    public static class CSharpCodeHelpers
    {
        /// <summary>
        /// Checks if the string passed in <paramref name="name"/> is a valid identifier
        /// </summary>
        /// <param name="name">The string to check</param>
        /// <returns>True if the name is a valid identifier, false otherwise.</returns>
        public static bool IsProperIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (char.IsDigit(name[0]))
                return false;

            for (var i = 0; i < name.Length; ++i)
            {
                var ch = name[i];
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the string passed in <paramref name="name"/> is empty, or a valid identifier
        /// </summary>
        /// <param name="name">The string to check</param>
        /// <returns>True if the string is empty, otherwise, returns the value obtained from <see cref="IsProperIdentifier(string)"/></returns>
        public static bool IsEmptyOrProperIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;

            return IsProperIdentifier(name);
        }

        /// <summary>
        /// Checks if the string passed in <paramref name="name"/> is empty, or a valid namespace name
        /// </summary>
        /// <param name="name">The string to check</param>
        /// <returns>true if the string is empty, otherwise, it splits the string using the '.' character and checks if all of them return true using .All(<see cref="IsProperIdentifier(string)"/>)"/></returns>
        public static bool IsEmptyOrProperNamespaceName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;

            return name.Split('.').All(IsProperIdentifier);
        }

        ////TODO: this one should add the @escape automatically so no other code has to worry
        /// <summary>
        /// Creates an identifier from the string passed in <paramref name="name"/>
        /// </summary>
        /// <param name="name">The string to transform into an identifier</param>
        /// <param name="suffix">A suffix to append to the identifier</param>
        /// <returns>an identifier based off the values given by the parameters</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty</exception>
        public static string MakeIdentifier(string name, string suffix = "")
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (char.IsDigit(name[0]))
                name = "_" + name;

            // See if we have invalid characters in the name.
            var nameHasInvalidCharacters = false;
            for (var i = 0; i < name.Length; ++i)
            {
                var ch = name[i];
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                {
                    nameHasInvalidCharacters = true;
                    break;
                }
            }

            // If so, create a new string where we remove them.
            if (nameHasInvalidCharacters)
            {
                var buffer = new StringBuilder();
                for (var i = 0; i < name.Length; ++i)
                {
                    var ch = name[i];
                    if (char.IsLetterOrDigit(ch) || ch == '_')
                        buffer.Append(ch);
                }

                name = buffer.ToString();
            }

            return name + suffix;
        }

        /// <summary>
        /// Creates an identifier using <see cref="MakeIdentifier(string, string)"/> and then formats it into camelCase
        /// </summary>
        /// <param name="name">The string to transform into an identifier</param>
        /// <param name="suffix">A suffix to append to the identifier</param>
        /// <returns>an identifier based off the values given by the parameters</returns>
        public static string MakeIdentifierCamelCase(string name, string suffix = "")
        {
            var str = MakeIdentifier(name, suffix);
            char[] chars = str.ToCharArray();
            chars[0] = char.ToLowerInvariant(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// Creates an identifier using <see cref="MakeIdentifier(string, string)"/> and then formats it into PascalCase
        /// </summary>
        /// <param name="name">The string to transform into an identifier</param>
        /// <param name="suffix">A suffix to append to the identifier</param>
        /// <returns>an identifier based off the values given by the parameters</returns>
        public static string MakeTypeName(string name, string suffix = "")
        {
            var symbolName = MakeIdentifier(name, suffix);
            if (char.IsLower(symbolName[0]))
                symbolName = char.ToUpper(symbolName[0]) + symbolName.Substring(1);
            return symbolName;
        }

        /// <summary>
        /// Creates a generic header comment to represent that the class inspected by the user was auto generated
        /// </summary>
        /// <param name="toolName">The name of the tool that created the code</param>
        /// <param name="toolVersion">The version of the tool that created the code</param>
        /// <param name="sourceFileName">The name of the file used as a source</param>
        /// <returns>The header</returns>
        public static string MakeAutoGeneratedCodeHeader(string toolName, string toolVersion, string sourceFileName = null)
        {
            return
                "//------------------------------------------------------------------------------\n"
                + "// <auto-generated>\n"
                + $"//     This code was auto-generated by {toolName}\n"
                + $"//     version {toolVersion}\n"
                + (string.IsNullOrEmpty(sourceFileName) ? "" : $"//     from {sourceFileName}\n")
                + "//\n"
                + "//     Changes to this file may cause incorrect behavior and will be lost if\n"
                + "//     the code is regenerated.\n"
                + "// </auto-generated>\n"
                + "//------------------------------------------------------------------------------\n";
        }

        public static string ToLiteral(this object value)
        {
            if (value == null)
                return "null";

            var type = value.GetType();

            if (type == typeof(bool))
            {
                if ((bool)value)
                    return "true";
                return "false";
            }

            if (type == typeof(char))
                return $"'\\u{(int)(char)value:X2}'";

            if (type == typeof(float))
                return value + "f";

            if (type == typeof(uint) || type == typeof(ulong))
                return value + "u";

            if (type == typeof(long))
                return value + "l";

            if (type.IsEnum)
            {
                var enumValue = type.GetEnumName(value);
                if (!string.IsNullOrEmpty(enumValue))
                    return $"{type.FullName.Replace("+", ".")}.{enumValue}";
            }

            return value.ToString();
        }
        public static string GetInitializersForPublicPrimitiveTypeFields(this object instance)
        {
            var type = instance.GetType();
            var defaults = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var fieldInits = string.Join(", ",
                fields.Where(f => (f.FieldType.IsPrimitive || f.FieldType.IsEnum) && !f.GetValue(instance).Equals(f.GetValue(defaults)))
                    .Select(f => $"{f.Name} = {f.GetValue(instance).ToLiteral()}"));

            if (string.IsNullOrEmpty(fieldInits))
                return "()";

            return " { " + fieldInits + " }";
        }
    }
}
