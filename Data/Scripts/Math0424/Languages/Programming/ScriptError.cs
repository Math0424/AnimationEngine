using System;
using System.Text;

namespace AnimationEngine.Language
{
    internal class ScriptError : Exception
    {
        StringBuilder StringBuilder = new StringBuilder();

        public ScriptError AppendError(string error, string line, int index)
        {
            StringBuilder.AppendLine(error);
            StringBuilder.AppendLine(line.Replace('\n', ' '));
            for (int i = 1; i < index; i++)
            {
                StringBuilder.Append(" ");
            }
            StringBuilder.Append("^\n");
            return this;
        }

        public ScriptError AppendError(string error)
        {
            StringBuilder.AppendLine(error);
            return this;
        }

        public ScriptError AppendError(Exception ex)
        {
            StringBuilder.AppendLine(ex.ToString());
            return this;
        }

        public override string ToString()
        {
            return StringBuilder.ToString();
        }
    }
}
