using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RoR2.Editor.CodeGen
{
    public struct Writer
    {
        public StringBuilder buffer;
        public int indentLevel;

        public void BeginBlock()
        {
            WriteIndent();
            buffer.Append("{\n");
            ++indentLevel;
        }

        public void EndBlock()
        {
            --indentLevel;
            WriteIndent();
            buffer.Append("}\n");
        }

        public void WriteLine()
        {
            buffer.Append('\n');
        }

        public void WriteVerbatim(string verbatimText)
        {
            string[] split = verbatimText.Split("\r\n", StringSplitOptions.None);
            foreach (string line in split)
            {
                WriteLine(line);
            }
        }
        public void WriteLine(string text)
        {
            if (!text.All(char.IsWhiteSpace))
            {
                WriteIndent();
                buffer.Append(text);
            }
            buffer.Append('\n');
        }

        public void Write(string text)
        {
            buffer.Append(text);
        }

        public void WriteIndent()
        {
            for (var i = 0; i < indentLevel; ++i)
            {
                for (var n = 0; n < 4; ++n)
                    buffer.Append(' ');
            }
        }

        public override string ToString()
        {
            return buffer.ToString();
        }
    }
}