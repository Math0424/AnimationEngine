using AnimationEngine.Language;
using System.Collections.Generic;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal abstract class CompilationNode
    {
        public List<CompilationNode> children = new List<CompilationNode>();
        protected ScriptV2Generator Script => Context.GetScript();
        protected Token[] Tokens => Context.GetTokens();

        public abstract void Compile();
        public abstract void PostCompile();
        public virtual void Cleanup()
        {
            foreach (var x in children)
                x.Cleanup();
        }

        protected int Next(TokenType token, int startIndex)
        {
            for (int i = startIndex; i < Tokens.Length; i++)
            {
                if (Tokens[i].Type == token)
                {
                    return i;
                }
            }
            return -1;
        }

        protected Token[] StrictVector(ref int index, TokenType stop)
        {
            if (Tokens[index].Type == stop)
            {
                return new Token[0];
            }

            List<Token> args = new List<Token>();
            int balance = 0;
            while (index < Tokens.Length)
            {
                if (Tokens[index].Type == TokenType.ENDL)
                {
                    throw Script.DetailedErrorLog($"Reached end of vector without closure", Tokens[index]);
                }

                if (Tokens[index].Type == stop)
                {
                    if (balance >= 0)
                    {
                        throw Script.DetailedErrorLog($"Too many vector seperators", Tokens[index]);
                    }
                    break;
                }

                if (Tokens[index].Type == TokenType.COM)
                {
                    balance++;
                }
                else
                {
                    balance--;
                }

                if (balance > 1)
                {
                    throw Script.DetailedErrorLog($"Too many vector seperators", Tokens[index]);
                }
                else if (balance < -1)
                {
                    throw Script.DetailedErrorLog($"Missing vector seperator", Tokens[index]);
                }

                if (Tokens[index].Type != TokenType.COM)
                {
                    args.Add(Tokens[index]);
                }
                index++;
            }
            return args.ToArray();
        }

    }
}
