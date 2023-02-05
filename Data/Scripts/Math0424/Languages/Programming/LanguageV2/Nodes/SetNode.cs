using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class SetNode : CompilationNode
    {
        Token t;
        bool global = false;
        bool _pushToStack;

        public SetNode(ref int index, bool pushToStack = false)
        {
            _pushToStack = pushToStack;

            t = Tokens[index];
            global = Context.IsGlobalVariable(t.Value.ToString());
            if (!pushToStack && !(global || Context.IsVariable(t.Value.ToString())))
            {
                throw Script.DetailedErrorLog("Cannot set a non variable", t);
            }

            index += 2;
            children.Add(new EquationNode(ref index, Tokens.Length));
        }

        public override void Compile()
        {
            children[0].Compile();
            if (!_pushToStack)
            {
                if (global)
                    Script.program.Add(new Line(ProgramFunc.StG, Context.GetGlobalVariable(t.Value.ToString())));
                else
                    Script.program.Add(new Line(ProgramFunc.Cpy, 0, Context.GetCompileVariableIndex(t.Value.ToString())));
                Script.program.Add(new Line(ProgramFunc.PopJ, 1));
                Context.PopStackIndex();
            }
        }

        public override void PostCompile() { }
    }
}