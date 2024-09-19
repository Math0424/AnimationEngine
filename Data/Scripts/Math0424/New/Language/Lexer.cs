using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static AnimationEngine.Data.Scripts.Math0424.New.Language.Lexer;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    internal static class LexerExt
    {
        public static int GetPrecedence(this LexerTokenValue value)
        {
            switch (value)
            {
                case LexerTokenValue.OR:
                    return 1;
                case LexerTokenValue.AND:
                    return 2;
                case LexerTokenValue.COMP:
                case LexerTokenValue.NOTEQ:
                    return 3;
                case LexerTokenValue.LST:
                case LexerTokenValue.GRT:
                case LexerTokenValue.LSTEQ:
                case LexerTokenValue.GRTEQ:
                    return 4;
                case LexerTokenValue.ADD:
                case LexerTokenValue.SUBTRACT:
                    return 5;
                case LexerTokenValue.MULTIPLY:
                case LexerTokenValue.DIVIDE:
                    return 6;
                default:
                    return 0;
            }
        }

        public static bool IsLiteralMatch(this LexerTokenValue value, LexerTokenValue value2)
        {
            switch (value)
            {
                case LexerTokenValue.LFLOAT:
                    return value2 == LexerTokenValue.FLOAT || value2 == LexerTokenValue.INT;
                case LexerTokenValue.LINT:
                    return value2 == LexerTokenValue.INT;
                case LexerTokenValue.LSTRING:
                    return value2 == LexerTokenValue.STRING;
                case LexerTokenValue.TRUE:
                    return value2 == LexerTokenValue.BOOL;
                case LexerTokenValue.FALSE:
                    return value2 == LexerTokenValue.BOOL;

                case LexerTokenValue.FLOAT:
                    return value2 == LexerTokenValue.LFLOAT || value2 == LexerTokenValue.LINT;
                case LexerTokenValue.INT:
                    return value2 == LexerTokenValue.LINT;
                case LexerTokenValue.STRING:
                    return value2 == LexerTokenValue.LSTRING;
                case LexerTokenValue.BOOL:
                    return value2 == LexerTokenValue.TRUE || value2 == LexerTokenValue.FALSE;

                default: return false;
            }
        }

        public static bool IsLiteralMathVariable(this LexerTokenValue value)
        {
            switch (value)
            {
                case LexerTokenValue.LFLOAT:
                case LexerTokenValue.LINT:
                case LexerTokenValue.TRUE:
                case LexerTokenValue.FALSE:
                    return true;
                default: return false;
            }
        }

        public static bool IsLiteralVariable(this LexerTokenValue value)
        {
            switch (value)
            {
                case LexerTokenValue.LFLOAT:
                case LexerTokenValue.LINT:
                case LexerTokenValue.LSTRING:
                case LexerTokenValue.TRUE:
                case LexerTokenValue.FALSE:
                    return true;
                default: return false;
            }
        }

        public static bool IsVariable(this LexerTokenValue value)
        {
            switch (value)
            {
                case LexerTokenValue.FLOAT:
                case LexerTokenValue.INT:
                case LexerTokenValue.STRING:
                case LexerTokenValue.BOOL:
                    return true;
                default: return false;
            }
        }

        public static bool IsMathOperator(this LexerTokenValue value)
        {
            switch (value)
            {
                case LexerTokenValue.EQUAL:
                case LexerTokenValue.ADD:
                case LexerTokenValue.SUBTRACT:
                case LexerTokenValue.MULTIPLY:
                case LexerTokenValue.DIVIDE:
                case LexerTokenValue.MODULO:
                case LexerTokenValue.INTDIVISION:
                    return true;
                default: return false;
            }
        }

        public static bool IsLogicOperator(this LexerTokenValue value)
        {
            switch (value)
            {
                case LexerTokenValue.COMP:
                case LexerTokenValue.NOTEQ:
                case LexerTokenValue.GRT:
                case LexerTokenValue.GRTEQ:
                case LexerTokenValue.LST:
                case LexerTokenValue.LSTEQ:
                case LexerTokenValue.AND:
                case LexerTokenValue.OR:
                    return true;
                default: return false;
            }
        }
    }

    internal class Lexer
    {
        static Regex _numberRegex = new Regex(@"^-?(([1-9]\d*|0)(\.\d*)?|\.\d+)", RegexOptions.Compiled);
        static Regex _wordRegex = new Regex(@"^[a-zA-Z_][a-zA-Z_0-9]*", RegexOptions.Compiled);
        static Regex _stringRegex = new Regex("^\"(.+?)\"", RegexOptions.Compiled);

        public enum LexerTokenValue
        {
            EMPTY,

            // variables
            LINT,
            LFLOAT,
            LSTRING,
            LBOOL,

            // keywords
            INT,
            FLOAT,
            STRING,
            BOOL,
            
            LET,

            STRUCT,
            FUNC,

            IF,
            ELSE,

            FOR,
            WHILE,
            SWITCH,
            DEFAULT,

            USING,
            AS,
            PARENT,

            BREAK,
            CONTINUE,
            CASE,
            RETURN,

            TRUE,
            FALSE,

            // grammar
            SEMICOLON,
            COLON,
            DOT,
            COMMA,
            AT,
            IMPORT,

            BKSLASH,

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

            SEPERATOR, // ::

            COMP,
            GRT,
            LST,
            GRTEQ,
            LSTEQ,

            NOT,
            AND,
            OR,
            NOTEQ,

            KEYWORD,
            UNKNOWN,

            // AST nodes
            HEADER,
            ROOT,
            VARIABLE,
            BODY,
            EXPRESSION,
            UNKNOWN_VARIABLE,

            FUNCTION_CALL,
            LIBRARY_CALL,
            OBJECT_CALL,
        }

        public struct LexerToken
        {
            public string File;
            public LexerTokenValue Type;
            public object RawValue;
            public int LineNumber, CharacterNumber;
            public LexerToken(string File, LexerTokenValue type, object rawValue, int line, int index)
            {
                this.File = File;
                this.Type = type;
                this.RawValue = rawValue;
                this.LineNumber = line;
                this.CharacterNumber = index;
            }
            public override string ToString()
            {
                return $"[{File}] {LineNumber + 1:000}:{CharacterNumber + 1:00} -> [{Type}]: {RawValue}";
            }
        }

        // Do nothing but load this class into memory
        // makes the timings for the first script more accurate
        public static void Init()
        {
            _wordRegex.Match("");
            _stringRegex.Match("");
            _numberRegex.Match("");
            new LexerToken("debug", LexerTokenValue.FLOAT, 0, 0, 0);
        }

        public static LexerToken[] Parse(string fileName, string[] file)
        {
            List<LexerToken> tokens = new List<LexerToken>();
            for (int line = 0; line < file.Length; line++)
            {
                string rawLine = file[line];
                if (string.IsNullOrEmpty(rawLine))
                    continue;

                bool noEndL = false;

                for (int index = 0; index < rawLine.Length; index++)
                {
                    char c = rawLine[index];
                    if (char.IsWhiteSpace(c)) continue;
                    if (c == '#') break;

                    string subString = rawLine.Substring(index);

                    var match = _wordRegex.Match(subString);
                    if (match.Success)
                    {
                        tokens.Add(new LexerToken(fileName, GetWordValue(match.Value), match.Value, line, index));
                        index += match.Length - 1;
                        continue;
                    }

                    match = _stringRegex.Match(subString);
                    if (match.Success)
                    {
                        tokens.Add(new LexerToken(fileName, LexerTokenValue.LSTRING, match.Groups[1].Value, line, index));
                        index += match.Length - 1;
                        continue;
                    }

                    match = _numberRegex.Match(subString);
                    if (match.Success)
                    {
                        if (match.Groups[2].Success && !match.Groups[3].Success)
                            tokens.Add(new LexerToken(fileName, LexerTokenValue.LINT, int.Parse(match.Value), line, index));
                        else
                            tokens.Add(new LexerToken(fileName, LexerTokenValue.LFLOAT, float.Parse(match.Value), line, index));
                        index += match.Length - 1;
                        continue;
                    }

                    LexerTokenValue token = GetOperator(ref rawLine, ref index);
                    if (token == LexerTokenValue.UNKNOWN)
                        RaiseError(file, line, index, $"Lexer cannot decipher token '{rawLine[index]}'");
                    if (token == LexerTokenValue.SEMICOLON && tokens[tokens.Count - 1].Type == LexerTokenValue.SEMICOLON)
                        continue;

                    noEndL = false;
                    if (token == LexerTokenValue.BKSLASH)
                    {
                        noEndL = true;
                        continue;
                    }
                    tokens.Add(new LexerToken(fileName, token, subString[0], line, index));

                }

                if (!noEndL && tokens[tokens.Count - 1].Type != LexerTokenValue.SEMICOLON)
                    tokens.Add(new LexerToken(fileName, LexerTokenValue.SEMICOLON, "EndL", line, rawLine.Length));
            }
            return tokens.ToArray();
        }

        static void RaiseError(string[] rawFile, int line, int character, string error)
        {
            throw new Exception($"Lexing Error at {line + 1}:{character + 1}\n{rawFile[line].Trim()}\n{"^".PadLeft(character)}\n-> {error}");
        }

        static LexerTokenValue GetWordValue(string str)
        {
            switch(str.ToLower())
            {
                case "int":
                    return LexerTokenValue.INT;
                case "float":
                    return LexerTokenValue.FLOAT;
                case "string":
                    return LexerTokenValue.STRING;
                case "bool":
                    return LexerTokenValue.BOOL;

                case "let":
                    return LexerTokenValue.LET;

                case "struct":
                    return LexerTokenValue.STRUCT;
                case "func":
                    return LexerTokenValue.FUNC;

                case "if":
                    return LexerTokenValue.IF;
                case "else":
                    return LexerTokenValue.ELSE;

                case "for":
                    return LexerTokenValue.FOR;
                case "while":
                    return LexerTokenValue.WHILE;
                case "switch":
                    return LexerTokenValue.SWITCH;
                case "case":
                    return LexerTokenValue.CASE;
                case "default":
                    return LexerTokenValue.DEFAULT;

                case "using":
                    return LexerTokenValue.USING;
                case "as":
                    return LexerTokenValue.AS;
                case "parent":
                    return LexerTokenValue.PARENT;
                case "import":
                    return LexerTokenValue.IMPORT;

                case "true":
                    return LexerTokenValue.TRUE;
                case "false":
                    return LexerTokenValue.FALSE;

                case "break":
                    return LexerTokenValue.BREAK;
                case "continue":
                    return LexerTokenValue.CONTINUE;
                case "return":
                    return LexerTokenValue.RETURN;

                default:
                    return LexerTokenValue.KEYWORD;
            }
        }

        private static LexerTokenValue GetOperator(ref string line, ref int index)
        {
            switch (line[index])
            {
                case ';':
                    return LexerTokenValue.SEMICOLON;
                case ',':
                    return LexerTokenValue.COMMA;
                case ':':
                    if (index + 1 < line.Length && line[index + 1] == ':')
                    {
                        index++;
                        return LexerTokenValue.SEPERATOR;
                    }
                    return LexerTokenValue.COLON;
                case '.':
                    return LexerTokenValue.DOT;
                case '@':
                    return LexerTokenValue.AT;
                case '{':
                    return LexerTokenValue.LBRACE;
                case '}':
                    return LexerTokenValue.RBRACE;
                case '(':
                    return LexerTokenValue.LPAREN;
                case ')':
                    return LexerTokenValue.RPAREN;
                case '[':
                    return LexerTokenValue.LSQBRC;
                case ']':
                    return LexerTokenValue.RSQBRC;

                case '\\':
                    return LexerTokenValue.BKSLASH;
                case '+':
                    return LexerTokenValue.ADD;
                case '-':
                    return LexerTokenValue.SUBTRACT;
                case '*':
                    return LexerTokenValue.MULTIPLY;
                case '/':
                    if (index + 1 < line.Length && line[index + 1] == '/')
                    {
                        index++;
                        return LexerTokenValue.INTDIVISION;
                    }
                    return LexerTokenValue.DIVIDE;
                case '%':
                    return LexerTokenValue.MODULO;

                case '<':
                    if (index + 1 < line.Length && line[index + 1] == '=')
                    {
                        index++;
                        return LexerTokenValue.GRT;
                    }
                    return LexerTokenValue.GRT;
                case '>':
                    if (index + 1 < line.Length && line[index + 1] == '=')
                    {
                        index++;
                        return LexerTokenValue.LST;
                    }
                    return LexerTokenValue.LST;
                case '=':
                    if (index + 1 < line.Length && line[index + 1] == '=')
                    {
                        index++;
                        return LexerTokenValue.COMP;
                    }
                    return LexerTokenValue.EQUAL;
                case '!':
                    if (index + 1 < line.Length && line[index + 1] == '=')
                    {
                        index++;
                        return LexerTokenValue.NOTEQ;
                    }
                    return LexerTokenValue.NOT;
                case '|':
                    if (index + 1 < line.Length && line[index + 1] == '|')
                    {
                        index++;
                        return LexerTokenValue.OR;
                    }
                    return LexerTokenValue.UNKNOWN;
                case '&':
                    if (index + 1 < line.Length && line[index + 1] == '&')
                    {
                        index++;
                        return LexerTokenValue.AND;
                    }
                    return LexerTokenValue.UNKNOWN;

            }
            return LexerTokenValue.UNKNOWN;
        }

    }
}
