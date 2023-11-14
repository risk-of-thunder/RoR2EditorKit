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
        public Writer BeginBlock()
        {
            WriteIndent();
            buffer.Append("{\n");
            ++indentLevel;
            return this;
        }

        /// <summary>
        /// Finishes a code block started with <see cref="BeginBlock"/> and indents the future text properly
        /// </summary>
        public Writer EndBlock()
        {
            --indentLevel;
            WriteIndent();
            buffer.Append("}\n");
            return this;
        }

        /// <summary>
        /// Writes a line with no text, only a '\n' character
        /// </summary>
        public Writer WriteLine()
        {
            buffer.Append('\n');
            return this;
        }

        /// <summary>
        /// Writes a line of code with the text given
        /// </summary>
        /// <param name="text">The text to write</param>
        public Writer WriteLine(string text)
        {
            if (!text.All(char.IsWhiteSpace))
            {
                WriteIndent();
                buffer.Append(text);
            }
            buffer.Append('\n');
            return this;
        }

        public Writer WritePreprocessorDirectiveLine(string directive)
        {
            if (!directive.All(char.IsWhiteSpace))
            {
                buffer.Append('#');
                buffer.Append(directive);
            }
            buffer.Append('\n');
            return this;
        }

        /// <summary>
        /// Writes code with the text given without appending a new line, useful for finishing existing lines.
        /// </summary>
        /// <param name="text">The text to write</param>
        public Writer Write(string text)
        {
            buffer.Append(text);
            return this;
        }

        /// <summary>
        /// Writes an indent
        /// </summary>
        public Writer WriteIndent()
        {
            for (var i = 0; i < indentLevel; ++i)
            {
                for (var n = 0; n < 4; ++n)
                    buffer.Append(' ');
            }
            return this;
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
