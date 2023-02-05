using AnimationEngine.Language;

namespace AnimationEngine.LanguageV1
{
    internal struct V1ScriptAction
    {
        public int ID;
        public Token Name;
        public Token[] Paramaters;
        public V1Function[] Funcs;
    }

    internal struct V1Function
    {
        public Token Name;
        public Token[] Paramaters;
        public V1Call[] Body;
    }

    internal struct V1Call
    {
        public TokenType Type;
        public Token Title;
        public V1Expression[] Expressions;
    }

    internal struct V1Expression
    {
        public Token Title;
        public Token[] Args;
    }
}
