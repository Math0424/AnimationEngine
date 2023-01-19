namespace AnimationEngine.Language
{
    internal enum TokenType
    {
        //special
        UKWN,

        INT,
        FLOAT,
        BOOL,
        
        STR,
        VECTOR,
        MVECTOR,

        OBJECT,
        FUNCCALL,
        BLOCK,

        //keywords
        KEWRD,
        USING,
        AS,
        FUNC,
        ACTION,
        VAR,
        LERP,

        //special keywords
        SUBPART,
        BUTTON,
        BASICIK,
        EMITTER,
        EMISSIVE,
        PARENT,
        LIGHT,

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

        ////math - TODO later
        EQL,  // =
        ADD,  // +
        SUB,  // -
        MUL,  // *
        DIV,  // /
        MOD,  // %

        IF, // if
        ELIF, // elif
        ELSE, // else
        
        COMP, // ==
        GRT,  // <
        LST,  // >
        GRTE, // <=
        LSTE, // >=
        
        NOT,  // !
        AND,  // &
        OR,   // |
        NOTEQ, // !=

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
    
}
