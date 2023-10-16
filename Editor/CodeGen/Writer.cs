using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoR2EditorKit.CodeGen
{
    /// <summary>
    /// Represents a text file being written.
    /// </summary>
    public struct Writer
    {
        /// <summary>
        /// The StringBuilder which contains the currently written text
        /// </summary>
        public StringBuilder buffer;
        /// <summary>
        /// The indent level of the writer
        /// </summary>
        public int indentLevel;

        /// <summary>
        /// Creates a new code block and indents the future text properly.
        /// </summary>
        public void BeginBlock()
        {
            WriteIndent();
            buffer.Append("{\n");
            ++indentLevel;
        }

        /// <summary>
        /// Finishes a code block started with <see cref="BeginBlock"/> and indents the future text properly
        /// </summary>
        public void EndBlock()
        {
            --indentLevel;
            WriteIndent();
            buffer.Append("}\n");
        }

        /// <summary>
        /// Writes a line with no text, only a '\n' character
        /// </summary>
        public void WriteLine()
        {
            buffer.Append('\n');
        }

        /// <summary>
        /// Writes a line of code with the text given
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
        /// Writes code with the text given without appending a new line, useful for finishing existing lines.
        /// </summary>
        /// <param name="text">The text to write</param>
        public void Write(string text)
        {
            buffer.Append(text);
        }

        /// <summary>
        /// Writes an indent
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
        /// Returns the written text file.
        /// </summary>
        public override string ToString()
        {
            return buffer.ToString();
        }
    }
}
