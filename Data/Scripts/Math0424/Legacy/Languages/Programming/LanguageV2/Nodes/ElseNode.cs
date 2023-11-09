using AnimationEngine.Language;
using AnimationEngine.Utility;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class ElseNode : CompilationNode
    {
        public ElseNode(ref int index)
        {
            if (Tokens[index].Type != TokenType.ELSE || index + 2 >= Tokens.Length)
            {
                throw Script.DetailedErrorLog("Not an ELSE statement", Tokens[index]);
            }
            
            if (Tokens[index + 1].Type == TokenType.ENDL)
                index++;

            bool end = Tokens[index + 1].Type == TokenType.VECTOR;
            bool ifs = Tokens[index + 1].Type == TokenType.IF;

            if (!(end || ifs))
                throw Script.DetailedErrorLog("Missing next ELSE statement", Tokens[index + 1]);
            
            index++;
            if (end)
                children.Add(new BodyNode(ref index));
            else
                children.Add(new IfNode(ref index));
        }

        public override void Compile()
        {
            children[0].Compile();
        }

        public override void PostCompile() 
        { 
            children[0].PostCompile();
        }
    }
}
