using System;
using System.Linq;
using System.Text;


namespace RoR2.Editor.CodeGen
{
    /// <summary>
    /// <see cref="Writer"/> is a struct that's used to write and store code. This struct can be passed to <see cref="CodeGeneratorValidator"/> using <see cref="CodeGeneratorValidator.Validate(CodeGeneratorValidator.ValidationData)"/> to write the code to disk
    /// </summary>
    public struct Writer
    {
        /// <summary>
        /// The buffer string builder that contains the code being written
        /// </summary>
        public StringBuilder buffer;

        /// <summary>
        /// How much indenting is being used for the current writing process.
        /// </summary>
        public int indentLevel;


        /// <summary>
        /// Begins a new CodeBlock, writing the indent, appending "{" to the buffer, then incrementing the indent level.
        /// <br>To close a CodeBlock, use <see cref="EndBlock"/></br>
        /// </summary>
        public void BeginBlock()
        {
            WriteIndent();
            buffer.Append("{\n");
            ++indentLevel;
        }

        /// <summary>
        /// Ends a code block, decrementing the indent level, writing the indent then appending "}".
        /// <br>Should be called if <see cref="BeginBlock"/> was used previously</br>
        /// </summary>
        public void EndBlock()
        {
            --indentLevel;
            WriteIndent();
            buffer.Append("}\n");
        }

        /// <summary>
        /// Writes a line break to the buffer
        /// </summary>
        public void WriteLine()
        {
            buffer.Append('\n');
        }

        /// <summary>
        /// Writes the incoming string as if it where a Verbatim string. (@""). interpolated verbatim strings are allowed (@$"")
        /// 
        /// <br>This is done by splitting the incoming string using <see cref="string.Split(char, StringSplitOptions)"/>, and using "\r\n" as the separator, then writing each split string using <see cref="WriteLine(string)"/></br>
        /// <br>Allows for complex bulk writing of members, methods, and more.</br>
        /// </summary>
        /// <param name="verbatimText">The text to write, which was written as a verbatim string</param>
        public void WriteVerbatim(string verbatimText)
        {
            string[] split = verbatimText.Split("\r\n", StringSplitOptions.None);
            foreach (string line in split)
            {
                WriteLine(line);
            }
        }

        /// <summary>
        /// Writes the incoming string as a line in the buffer
        /// </summary>
        /// <param name="text">The text to write</param>
        public void WriteLine(string text)
        {
            if (!text.All(char.IsWhiteSpace))
            {
                WriteIndent();
                buffer.Append(text);
            }
            buffer.Append('\n');
        }

        /// <summary>
        /// Directyl appends the incoming string to the buffer
        /// </summary>
        /// <param name="text">The text to append</param>
        public void Write(string text)
        {
            buffer.Append(text);
        }

        /// <summary>
        /// Writes an indent to the buffer
        /// </summary>
        public void WriteIndent()
        {
            for (var i = 0; i < indentLevel; ++i)
            {
                for (var n = 0; n < 4; ++n)
                    buffer.Append(' ');
            }
        }

        /// <summary>
        /// Returns the <see cref="buffer"/>'s contents as a string
        /// </summary>
        /// <returns>The written code</returns>
        public override string ToString()
        {
            return buffer.ToString();
        }
    }
}