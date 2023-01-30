using AnimationEngine.Language;
using System;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class ObjectNode : CompilationNode
    {
        public ObjectNode(int start)
        {
            int length = Next(TokenType.ENDL, start) - start;
            if (length < 4)
            {
                throw Script.DetailedErrorLog($"Invalid object format missing sections!", Tokens[start]);
            }
            else if (Tokens[start + 2].Type != TokenType.AS)
            {
                throw Script.DetailedErrorLog($"Invalid object format!", Tokens[start + 2]);
            }

            string name = Tokens[start + 1].Value.ToString().ToLower();
            if (Script.objects.Exists((e) => e.Name.Value.ToString().ToLower().Equals(name)))
            {
                throw Script.DetailedErrorLog($"Duplicate object declaration", Tokens[start + 1]);
            }

            Entity obj = new Entity()
            {
                Name = Tokens[start + 1],
                Type = Tokens[start + 3],
            };

            int c = start + 4;
            if (length >= 5)
            {
                if (Tokens[c].Type != TokenType.LPAREN)
                {
                    throw Script.DetailedErrorLog($"Missing parentheses", Tokens[c]);
                }

                c++;
                obj.Args = StrictVector(ref c, TokenType.RPAREN);
                c++;

                if (Tokens[c].Type == TokenType.PARENT)
                {
                    if (Tokens[c + 1].Type != TokenType.KEWRD)
                    {
                        throw Script.DetailedErrorLog($"Object requires parent name", Tokens[c + 1]);
                    }
                    if (!Script.objects.Exists((e) => e.Name.Value.ToString().ToLower().Equals(Tokens[c + 1].Value.ToString().ToLower())))
                    {
                        throw Script.DetailedErrorLog($"Cannot find parent, located below or missing?", Tokens[c + 1]);
                    }
                    c += 2;
                }
            }

            if (Tokens[c].Type != TokenType.ENDL)
            {
                throw Script.DetailedErrorLog($"Extra values at end", Tokens[c]);
            }

            Script.objects.Add(obj);
        }

        public override void Compile()
        {
            //nothing
        }

        public override void PostCompile()
        {

        }
    }
}
