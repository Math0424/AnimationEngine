using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    internal class Lexer
    {
        public enum LexerTokenValue
        {
            // keywords
            INT,
            FLOAT,
            STRING,
            BOOL,
            
            VAR,

            STRUCT,
            FUNC,

            IF,
            ELIF,
            ELSE,

            FOR,
            WHILE,
            SWITCH,
            DEFAULT,

            BREAK,
            CONTINUE,
            RETURN,

            // grammar
            ENDL,
            SEMICOLON,
            DOT,
            COMMA,
            AMPERSAND,

            LBRACE,
            RBRACE,
            LPAREN,
            RPAREN,
            RSQBRC,
            LSQBRC,

            // operators
            EQUAL,       // =
            ADD,         // +
            SUBTRACT,    // -
            MULTIPLY,    // *
            DIVIDE,      // /
            MODULO,      // %

            INTDIVISION, // //

            CMP,
            GRT,
            LST,
            GRTEQ,
            LSTEQ,

            NOT,
            AND,
            OR,
            NOTEQ,

            UNKNOWN,
        }

        public struct LexerToken
        {
            LexerTokenValue type;
            string rawValue;
            int lineNumber, characterNumber;
        }
        
        public static LexerToken[] Parse(string[] file)
        {
            return null;
        }


    }
}
