using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class VariableNode : CompilationNode
    {
        public string Name;

        public VariableNode(ref int index)
        {
            if (index + 5 > Tokens.Length || Tokens[index + 2].Type != TokenType.EQL)
            {
                throw Script.DetailedErrorLog($"Invalid variable definition (var [name] = [value])", Tokens[index]);
            }

            Name = Tokens[index + 1].Value.ToString().ToLower();
            if (Context.IsVariable(Name) || Context.IsGlobalVariable(Name))
            {
                throw Script.DetailedErrorLog($"Duplicate variable name", Tokens[index + 1]);
            }

            index += 1;
            Context.RequireReturn = true;
            children.Add(new SetNode(ref index, true));
            Context.RequireReturn = false;
            Context.AddVariable(Name);
        }

        public override void Compile()
        {
            children[0].Compile();
            Context.AddCompileVariable(Name);
        }

        public override void PostCompile()
        {
            Context.RemoveCompileVariable(Name);
            Context.PopStackIndex();
            children[0].PostCompile();
            Script.program.Add(new Line(ProgramFunc.PopJ, 1));
        }
    }
}
