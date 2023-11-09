using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class WhileNode : CompilationNode
    {
        LogicNode logic;
        CompilationNode body;

        public WhileNode(ref int index)
        {
            if (Tokens[index].Type != TokenType.WHILE || index + 3 >= Tokens.Length)
            {
                throw Script.DetailedErrorLog("Not an WHILE statement", Tokens[index]);
            }

            if (Tokens[index + 1].Type != TokenType.LPAREN)
            {
                throw Script.DetailedErrorLog("Missing WHILE opening parentheses", Tokens[index + 1]);
            }

            index++;
            Context.RequireReturn = true;
            children.Add(new LogicNode(ref index));
            logic = (LogicNode)children[children.Count - 1];
            Context.RequireReturn = false;

            if (Tokens[index].Type != TokenType.VECTOR)
            {
                throw Script.DetailedErrorLog("Missing WHILE closing parentheses ", Tokens[index]);
            }

            children.Add(new BodyNode(ref index));
            body = children[children.Count - 1];
        }

        public override void Compile()
        {
            int topJumpIndex = Script.program.Count;
            Script.program.Add(new Line(ProgramFunc.B));

            body.Compile();
            body.PostCompile();

            int jumpInd = Script.program.Count;
            logic.Compile();
            Context.PopStackIndex();
            Script.program.Add(new Line(ProgramFunc.Cmp, 0));
            Script.program.Add(new Line(ProgramFunc.PopJ, 1));
            Script.program.Add(new Line(ProgramFunc.BZ, topJumpIndex + 1));
            logic.PostCompile();

            Script.program[topJumpIndex] = new Line(ProgramFunc.B, jumpInd);
        }

        public override void PostCompile() { }
    }
}
