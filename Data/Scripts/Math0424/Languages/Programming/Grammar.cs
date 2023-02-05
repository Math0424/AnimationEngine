namespace AnimationEngine.Language
{
    internal enum TokenType
    {
        INT,
        FLOAT,
        BOOL,
        STR,
        
        VECTOR,
        MVECTOR,

        SYSGET, // system get
        SYSSET, // system set

        // used for parsing in functions
        FUNCCALL,
        OBJECTCALL,

        //keywords
        LERP,
        KEWRD,
        USING,
        VAR,
        AS,
        FUNC,
        ACTION,
        RETURN, 
        PARENT,
        TERMINAL,

        //operators
        DOT,
        COL,
        AT,
        TAB,
        ENDL,
        COM,

        LBRACE,
        RBRACE,
        LPAREN,
        RPAREN,

        RSQBRC,
        LSQBRC,

        ////math
        EQL,  // =
        ADD,  // +
        SUB,  // -
        MUL,  // *
        DIV,  // /
        MOD,  // %

        IF, // if
        ELSE, // else
        WHILE, // while

        COMP, // ==
        GRT,  // <
        LST,  // >
        GRTE, // <=
        LSTE, // >=

        NOT,  // !
        AND,  // &
        OR,   // |
        NOTEQ, // !=

        UKWN,
    }

    internal struct Token
    {
        public int Line, Col;
        public TokenType Type;
        public object Value;
        public Token(TokenType Type, object Value, int line, int col)
        {
            this.Line = line;
            this.Col = col;
            this.Type = Type;
            this.Value = Value;
        }
    }

    internal struct ScriptAction
    {
        public string TokenName;
        public Token Name;
        public Token[] Paramaters;
        public Function[] Funcs;
        public int ID;
    }

    internal struct Function
    {
        public string TokenName;
        public Token Name;
        public Token[] Paramaters;
    }
    
    internal struct Entity
    {
        public Entity(string name)
        {
            Type = new Token(TokenType.UKWN, name, 0, 0);
            Parent = Type;
            Name = Type;
            Args = null;
        }
        public Token Type;
        public Token Parent;
        public Token Name;
        public Token[] Args;
    }

}
