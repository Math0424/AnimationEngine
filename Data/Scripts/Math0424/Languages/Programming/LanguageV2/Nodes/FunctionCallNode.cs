using AnimationEngine.Language;
using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class FunctionCallNode : CompilationNode
    {
        Token t;
        string objectName;
        int variables;

        public FunctionCallNode(ref int index)
        {
            if (Tokens[index].Type != TokenType.KEWRD)
            {
                throw Script.DetailedErrorLog("Function call must be a name", Tokens[index]);
            }
            if (Tokens[index + 1].Type != TokenType.LPAREN)
            {
                throw Script.DetailedErrorLog("Function call must open with '('", Tokens[index + 1]);
            }

            t = Tokens[index];
            objectName = "func_" + t.Value.ToString().ToLower();

            index += 2;
            int balance = 1;
            while (Tokens[index].Type != TokenType.RPAREN)
            {
                if (Tokens[index].Type == TokenType.ENDL)
                    throw Script.DetailedErrorLog($"Reached end without closure", Tokens[index]);

                if (Tokens[index].Type == TokenType.LPAREN)
                    throw Script.DetailedErrorLog($"Too many parentheses", Tokens[index]);

                if (Tokens[index].Type == TokenType.COM)
                    balance++;
                else
                    balance--;

                if (balance > 1)
                    throw Script.DetailedErrorLog($"Too many commas", Tokens[index]);
                else if (balance < -1)
                    throw Script.DetailedErrorLog($"Missing comma", Tokens[index]);

                if (Tokens[index].Type != TokenType.COM)
                {
                    variables++;
                    int end = FindClose(index);
                    children.Add(new EquationNode(ref index, end));
                }
                else
                {
                    index++;
                }
            }
        }

        protected int FindClose(int start)
        {
            int balance = 0;
            while (start < Tokens.Length)
            {
                if (Tokens[start].Type == TokenType.LPAREN)
                    balance--;
                if (Tokens[start].Type == TokenType.RPAREN)
                    balance++;
                if ((balance == 0 && Tokens[start].Type == TokenType.COM) || balance == 1)
                    return start;
                start++;
            }
            return -1;
        }

        int index;
        public override void Compile()
        {
            foreach (var x in children)
            {
                x.Compile();
            }
            index = Script.program.Count;
            Script.program.Add(new Line(ProgramFunc.Jmp));
            for(int i = 0; i < variables; i++)
            {
                Context.PopStackIndex();
            }
            Context.IncreaseStackIndex();
        }

        public override void PostCompile()
        {
            if (!Script.functions.ContainsKey(objectName))
            {
                throw Script.DetailedErrorLog("Cannot find function name (misspelled or below current method)", t);
            }
            if (Script.functions[objectName].Paramaters.Length != variables)
            {
                throw Script.DetailedErrorLog("Missmatch of paramater count when calling function", t);
            }
            if (!Context.RequireReturn)
            {
                Context.PopStackIndex();
                Script.program.Add(new Line(ProgramFunc.PopJ, 1));
            }
        }

        public override void Cleanup()
        {
            Script.program[index] = new Line(ProgramFunc.Jmp, Script.methodLookup[objectName]);
            foreach (var x in children)
                x.Cleanup();
        }
    }
}
