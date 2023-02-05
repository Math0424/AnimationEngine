using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class IfNode : CompilationNode
    {

        LogicNode logic;
        CompilationNode body;

        public IfNode(ref int index)
        {
            if (Tokens[index].Type != TokenType.IF || index + 3 >= Tokens.Length)
            {
                throw Script.DetailedErrorLog("Not an IF statement", Tokens[index]);
            }

            if (Tokens[index + 1].Type != TokenType.LPAREN)
            {
                throw Script.DetailedErrorLog("Missing IF opening parentheses", Tokens[index + 1]);
            }

            index++;
            Context.RequireReturn = true;
            children.Add(new LogicNode(ref index));
            logic = (LogicNode)children[children.Count - 1];
            Context.RequireReturn = false;

            if (Tokens[index].Type != TokenType.VECTOR)
            {
                throw Script.DetailedErrorLog("Missing IF closing parentheses ", Tokens[index]);
            }

            children.Add(new BodyNode(ref index));
            body = children[children.Count - 1];
        }

        public override void Compile()
        {
            logic.Compile();

            Context.PopStackIndex();
            Script.program.Add(new Line(ProgramFunc.Cmp, 0));
            Script.program.Add(new Line(ProgramFunc.PopJ, 1));
            int top = Script.program.Count;
            Script.program.Add(new Line(ProgramFunc.BNZ, 0));

            logic.PostCompile();

            body.Compile();
            body.PostCompile();

            Script.program[top] = new Line(ProgramFunc.BNZ, Script.program.Count);
        }

        public override void PostCompile() { }

    }
}
