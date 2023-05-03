using AnimationEngine.Language;
using AnimationEngine.Utility;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class IfNode : CompilationNode
    {

        LogicNode logic;
        CompilationNode body;
        ElseNode elseNode;

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
            logic = new LogicNode(ref index);
            children.Add(logic);
            Context.RequireReturn = false;

            if (Tokens[index].Type == TokenType.ENDL)
                index++;

            if (Tokens[index].Type != TokenType.VECTOR)
                throw Script.DetailedErrorLog($"Missing IF closing parentheses", Tokens[index]);

            body = new BodyNode(ref index);
            children.Add(body);

            if (Tokens[index+1].Type == TokenType.ENDL)
                index++;

            if (index + 1 < Tokens.Length && Tokens[index+1].Type == TokenType.ELSE) {
                index++;
                elseNode = new ElseNode(ref index);
                children.Add(elseNode);
            }
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

            int elseTop = Script.program.Count;
            if (elseNode != null)
                Script.program.Add(new Line(ProgramFunc.B, 0));

            Script.program[top] = new Line(ProgramFunc.BNZ, Script.program.Count);

            if (elseNode != null)
            {
                elseNode.Compile();
                elseNode.PostCompile();

                Script.program[elseTop] = new Line(ProgramFunc.B, Script.program.Count);
            }
        }

        public override void PostCompile() { }

    }
}
