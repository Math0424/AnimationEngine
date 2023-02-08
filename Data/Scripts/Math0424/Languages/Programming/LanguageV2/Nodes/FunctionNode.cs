using AnimationEngine.Language;
using AnimationEngine.Utility;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class FunctionNode : CompilationNode
    {
        string name;
        Function func;

        public FunctionNode(ref int start, string append)
        {
            int length = Next(TokenType.ENDL, start) - start;
            if (length < 4)
            {
                throw Script.DetailedErrorLog($"Invalid function format missing sections!", Tokens[start]);
            }

            if (Tokens[start].Type != TokenType.KEWRD || Tokens[start + 1].Type != TokenType.LPAREN)
            {
                throw Script.DetailedErrorLog($"Invalid function format", Tokens[start]);
            }

            name = append + Tokens[start].Value.ToString().ToLower();
            if (Script.functions.ContainsKey(name))
            {
                throw Script.DetailedErrorLog($"Function has multiple declarations", Tokens[start]);
            }

            func = new Function()
            {
                TokenName = Tokens[start].Value.ToString().ToLower(),
                Name = Tokens[start],
            };
            start += 2;
            func.Paramaters = StrictVector(ref start, TokenType.RPAREN);
            func.Name.Value = name;

            foreach (var x in func.Paramaters)
            {
                if (x.Type != TokenType.KEWRD)
                    throw Script.DetailedErrorLog($"Function input must be keyword", x);
                string xName = x.Value.ToString().ToLower();
                if (Context.IsVariable(xName))
                    throw Script.DetailedErrorLog($"Cannot have duplicate variable names in function input", x);
                Context.AddVariable(xName);
            }

            start++;
            children.Add(new BodyNode(ref start));

            foreach (var x in func.Paramaters)
            {
                string xName = x.Value.ToString().ToLower();
                Context.RemoveVariable(xName);
            }

            Script.functions[name] = func;
        }

        int top = 0;
        public override void Compile()
        {
            top = Script.program.Count;
            Script.methodLookup[name] = Script.program.Count;
            foreach (var z in func.Paramaters)
            {
                Context.IncreaseStackIndex();
                Context.AddCompileVariable(z.Value.ToString());
            }
            foreach (var x in children)
                x.Compile();
        }

        public override void PostCompile()
        {
            foreach (var x in children)
                x.PostCompile();
            foreach (var z in func.Paramaters)
            {
                Context.RemoveCompileVariable(z.Value.ToString());
                Context.PopStackIndex();
                Script.program.Add(new Line(ProgramFunc.PopJ, 1));
            }
            Optimize();
            Script.program.Add(new Line(ProgramFunc.LdI, Script.AddImmediate(new SVariableInt(0))));
            Script.program.Add(new Line(ProgramFunc.End));
        }

        private void Optimize()
        {
            int index = Script.program.Count - 1;
            int popAmount = 0;
            while (index > top)
            {
                Line l = Script.program[index];
                if (l.Arg == ProgramFunc.PopJ)
                {
                    popAmount++;
                }
                else if (popAmount != 0 && l.Arg != ProgramFunc.PopJ)
                {
                    Script.program[index + 1].Arr[0] = popAmount;
                    popAmount = 0;
                }
                index--;
            }
        }

    }
}
