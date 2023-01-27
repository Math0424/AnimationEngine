using AnimationEngine.CoreScript;
using AnimationEngine.Language;
using AnimationEngine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class ScriptNode : CompilationNode
    {

        public ScriptNode()
        {
            AssembleMathVectors(); // turn [0,0,0] int MVECTOR
            AssembleBodys(0); // turn { } into VECTOR

            AssembleObjects();
            AssembleGlobals();

            AssembleFunctions();
            AssembleActions();
            AssembleTerminals();
        }

        private void AssembleTerminals()
        {
            int next = Next(TokenType.TERMINAL, 0);
            while (next != -1)
            {
                children.Add(new ActionNode(ref next, false));
                next = Next(TokenType.TERMINAL, next);
            }
        }

        private void AssembleActions()
        {
            int next = Next(TokenType.ACTION, 0);
            while (next != -1)
            {
                children.Add(new ActionNode(ref next, true));
                next = Next(TokenType.ACTION, next);
            }
        }

        private void AssembleFunctions()
        {
            int next = Next(TokenType.FUNC, 0);
            while (next != -1)
            {
                next++;
                children.Add(new FunctionNode(ref next, "func_"));
                next = Next(TokenType.FUNC, next);
            }
        }

        private void AssembleObjects()
        {
            int next = Next(TokenType.USING, 0);
            while (next != -1)
            {
                children.Add(new ObjectNode(next));
                next = Next(TokenType.USING, ++next);
            }
        }

        private void AssembleGlobals()
        {
            int next = Next(TokenType.VAR, 0);
            while(next != -1)
            {
                children.Add(new GlobalNode(next));
                next++;
                next = Next(TokenType.VAR, next);
            }
        }

        private void AssembleMathVectors()
        {
            int next = Next(TokenType.LSQBRC, 0);
            while (next != -1)
            {
                if (Next(TokenType.RSQBRC, next) == -1)
                {
                    throw Script.DetailedErrorLog($"Missing closing bracket", Tokens[next]);
                }

                int begin = next++;
                Token[] arr = StrictVector(ref next, TokenType.RSQBRC);

                if (arr.Length != 3)
                {
                    throw Script.DetailedErrorLog("Math vectors must be length 3", Tokens[next]);
                }

                foreach (var x in arr)
                {
                    if (x.Type != TokenType.INT && x.Type != TokenType.FLOAT)
                    {
                        throw Script.DetailedErrorLog($"Value must be a number, found {x.Type}", x);
                    }
                }

                Token t = new Token(TokenType.MVECTOR, arr.ToVector3(), arr[0].Line, arr[0].Col);
                Context.RemoveTokenRange(begin, next);

                Tokens[begin] = t;
                next -= next - begin;

                next = Next(TokenType.LSQBRC, next);
            }
        }

        private void AssembleBodys(int start)
        {
            if (start > Tokens.Length)
                return;

            start = Next(TokenType.LBRACE, start);

            if (start == -1)
                return;

            int end = Next(TokenType.RBRACE, start);
            if (end == -1)
                throw Script.DetailedErrorLog("Missing closing bracket", Tokens[start]);

            if (end > Next(TokenType.LBRACE, start))
            {
                AssembleBodys(start + 1);
            }

            end = Next(TokenType.RBRACE, start);
            if (end == -1)
                throw Script.DetailedErrorLog("Missing closing bracket", Tokens[start]);

            List<Token> args = new List<Token>();
            for (int i = start + 1; i < end; i++)
            {
                args.Add(Tokens[i]);
            }

            Context.RemoveTokenRange(start, end);
            Tokens[start] = new Token(TokenType.VECTOR, args.ToArray(), Tokens[start].Line, Tokens[start].Col);

            AssembleBodys(++start);
        }

        public override void Compile()
        {
            foreach (var x in children)
            {
                x.Compile();
                x.PostCompile();
                Context.ResetStack();
            }
            foreach (var x in children)
                x.Cleanup();
        }

        public override void PostCompile() { }

    }
}
