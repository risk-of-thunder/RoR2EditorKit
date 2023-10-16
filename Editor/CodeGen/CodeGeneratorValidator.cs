﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.CodeGen
{
    /// <summary>
    /// The CodeGeneratorValidator is a class that takes the Core written in a <see cref="Writer"/> struct and creates a text file for it in a desired path
    /// </summary>
    public static class CodeGeneratorValidator
    {
        /// <summary>
        /// Represents a Writer code that needs to be validated
        /// </summary>
        public struct ValidationData
        {
            /// <summary>
            /// The writer that contains the code
            /// </summary>
            public Writer code;
            /// <summary>
            /// The desired path of the file where the code will be stored at
            /// </summary>
            public string desiredPath;
        }

        /// <summary>
        /// Validates a <see cref="ValidationData"/>
        /// </summary>
        /// <param name="data">The data to validate</param>
        /// <returns>True if the File with the <see cref="ValidationData.code"/> writer has been written into a file, false otherwise.</returns>
        public static bool Validate(ValidationData data)
        {
            var code = data.code.ToString();

            if (File.Exists(data.desiredPath))
            {
                var existingCode = File.ReadAllText(data.desiredPath);
                if (existingCode == code || WithAllWhitespaceStripped(existingCode) == WithAllWhitespaceStripped(code))
                    return false;
            }

            CheckOut(data.desiredPath, code);
            return true;
        }

        private static void CheckOut(string path, string code)
        {
            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("data.desiredPath");

            // Make path relative to project folder.
            var projectPath = Application.dataPath;
            if (path.StartsWith(projectPath) && path.Length > projectPath.Length &&
                (path[projectPath.Length] == '/' || path[projectPath.Length] == '\\'))
                path = path.Substring(0, projectPath.Length + 1);
            AssetDatabase.MakeEditable(path);

            File.WriteAllText(path, code);
        }

        private static string WithAllWhitespaceStripped(string str)
        {
            var buffer = new StringBuilder();
            foreach(var ch in str)
            {
                if(!char.IsWhiteSpace(ch))
                {
                    buffer.Append(ch);
                }
            }
            return buffer.ToString();
        }
    }
}
