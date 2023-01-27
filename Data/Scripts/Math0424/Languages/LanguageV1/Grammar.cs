namespace AnimationEngine.LanguageV1
{

    internal struct ScriptAction
    {
        public int ID;
        public Token Name;
        public Token[] Paramaters;
        public Function[] Funcs;
    }

    internal struct Function
    {
        public Token Name;
        public Token[] Paramaters;
        public Call[] Body;
    }

    internal struct Call
    {
        public TokenType Type;
        public Token Title;
        public Expression[] Expressions;
    }

    internal struct Expression
    {
        public Token Title;
        public Token[] Args;
    }
    
    internal struct Entity
    {
        public Token Name;
        public TokenType Type;
        public Token[] Args;
    }

}
