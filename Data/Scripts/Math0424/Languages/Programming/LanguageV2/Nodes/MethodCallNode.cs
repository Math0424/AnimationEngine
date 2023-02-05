using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class MethodCallNode : CompilationNode
    {
        string methodName;
        MethodCallNode next;
        int variables;

        public MethodCallNode(ref int index)
        {
            if (Tokens[index].Type != TokenType.KEWRD)
            {
                throw Script.DetailedErrorLog("Method call must be a name", Tokens[index]);
            }
            if (Tokens[index + 1].Type != TokenType.LPAREN)
            {
                throw Script.DetailedErrorLog("Method call must open with '('", Tokens[index + 1]);
            }
            methodName = Tokens[index].Value.ToString();
            index += 2;

            int balance = 1;
            while(Tokens[index].Type != TokenType.RPAREN)
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

            bool match;
            MethodDictionary? d = LanguageDictionary.IsMethod(Context.ClassContext, methodName, variables, out match);
            if (!d.HasValue)
            {
                throw Script.DetailedErrorLog($"Cannot find method of name {Context.ClassContext}.{methodName}()", Tokens[index]);
            } 
            else if(!match)
            {
                string var = "";
                foreach(var x in d.Value.Tokens)
                {
                    var += x + ", ";
                }
                throw Script.DetailedErrorLog($"Error at {Context.ClassContext}.{methodName}({var.Trim().Substring(0, var.Length - 2)}), expected {d.Value.TokenCount} tokens, found {variables}", Tokens[index]);
            }

            if (Tokens.Length > index + 1 && Tokens[index + 1].Type == TokenType.DOT)
            {
                index += 2;
                if (Context.RequireReturn)
                {
                    throw Script.DetailedErrorLog("Method call must be a return type", Tokens[index]);
                }
                next = new MethodCallNode(ref index);
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

        public override void Compile()
        {
            foreach (var x in children)
                x.Compile(); 
            Context.IncreaseStackIndex();
            Script.program.Add(new Line(ProgramFunc.Mth, Script.AddImmediate(new SVariableString(methodName)), variables));
            next?.Compile();
        }

        public override void PostCompile()
        {
            foreach (var x in children)
            {
                x.PostCompile();
            }
            next?.PostCompile();
            if (!Context.RequireReturn)
            {
                Context.PopStackIndex();
                Script.program.Add(new Line(ProgramFunc.PopJ, 1));
            }
        }

    }
}
