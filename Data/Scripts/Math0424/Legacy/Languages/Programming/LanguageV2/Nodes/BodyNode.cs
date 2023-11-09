using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class BodyNode : CompilationNode
    {
        bool hasReturn = false;
        Token returnTok;

        public BodyNode(ref int start)
        {
            if (Tokens[start].Type != TokenType.VECTOR)
            {
                throw Script.DetailedErrorLog($"Missing body", Tokens[start]);
            }

            int c = 0;
            Context.EnterNewContext((Token[])Tokens[start].Value);
            while (c < Tokens.Length)
            {
                switch (Tokens[c].Type)
                {
                    case TokenType.VAR:
                        children.Add(new VariableNode(ref c));
                        break;
                    case TokenType.KEWRD:
                        if (Tokens[c + 1].Type == TokenType.EQL)
                        {
                            Context.RequireReturn = true;
                            children.Add(new SetNode(ref c));
                            Context.RequireReturn = false;
                        }
                        else
                        {
                            Context.RequireReturn = false;
                            children.Add(new ClassCallNode(ref c));
                        }
                        break;
                    case TokenType.IF:
                        children.Add(new IfNode(ref c));
                        break;
                    case TokenType.WHILE:
                        children.Add(new WhileNode(ref c));
                        break;
                    case TokenType.RETURN:
                        if (hasReturn)
                        {
                            throw Script.DetailedErrorLog("Cannot have multiple returns in one body", Tokens[c]);
                        }
                        returnTok = Tokens[c];
                        hasReturn = true;
                        children.Add(new ReturnNode(ref c));
                        break;
                    case TokenType.ENDL:
                        break;
                    default:
                        throw Script.DetailedErrorLog($"Unexpected token in body {Tokens[c].Type}", Tokens[c]);
                }
                c++;
            }
            Context.PopTopContext();
        }

        public override void Compile()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var x = children[i];
                if (x.GetType() == typeof(ReturnNode))
                {
                    if (i != children.Count - 1)
                    {
                        throw Script.DetailedErrorLog("Return statement must be at end of body", returnTok);
                    }
                    x.Compile();
                }
                else
                {
                    x.Compile();
                }
            }
        }

        public override void PostCompile()
        {
            foreach (var x in children)
                x.PostCompile();
        }

    }
}
