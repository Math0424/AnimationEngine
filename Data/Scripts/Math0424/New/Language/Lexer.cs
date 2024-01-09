using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    internal class Lexer
    {
        static Regex _numberRegex = new Regex(@"^-?(([1-9]\d*|0)(\.\d*)?|\.\d+)", RegexOptions.Compiled);
        static Regex _wordRegex = new Regex(@"^[a-zA-Z_]+", RegexOptions.Compiled);
        static Regex _stringRegex = new Regex("^\"(.+?)\"", RegexOptions.Compiled);

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

            USING,
            AS,
            PARENT,

            BREAK,
            CONTINUE,
            RETURN,

            TRUE,
            FALSE,

            // grammar
            ENDL,
            SEMICOLON,
            COLON,
            DOT,
            COMMA,
            AT,

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
        }

        public struct LexerToken
        {
            public LexerTokenValue Type;
            public object RawValue;
            public int LineNumber, CharacterNumber;
            public LexerToken(LexerTokenValue type, object rawValue, int line, int index)
            {
                this.Type = type;
                this.RawValue = rawValue;
                this.LineNumber = line;
                this.CharacterNumber = index;
            }
            public override string ToString()
            {
                return $"{LineNumber + 1:000}:{CharacterNumber + 1:00} -> [{Type}]: {RawValue}";
            }
        }
        
        public static LexerToken[] Parse(string[] file)
        {
            List<LexerToken> tokens = new List<LexerToken>();
            for (int line = 0; line < file.Length; line++)
            {
                string rawLine = file[line];
                if (string.IsNullOrEmpty(rawLine))
                    continue;

                for (int index = 0; index < rawLine.Length; index++)
                {
                    char c = rawLine[index];
                    if (char.IsWhiteSpace(c)) continue;
                    if (c == '#') break;

                    string subString = rawLine.Substring(index);

                    var match = _wordRegex.Match(subString);
                    if (match.Success)
                    {
                        tokens.Add(new LexerToken(GetWordValue(match.Value), match.Value, line, index));
                        index += match.Length - 1;
                        continue;
                    }

                    match = _stringRegex.Match(subString);
                    if (match.Success)
                    {
                        tokens.Add(new LexerToken(LexerTokenValue.STRING, match.Groups[1].Value, line, index));
                        index += match.Length - 1;
                        continue;
                    }

                    match = _numberRegex.Match(subString);
                    if (match.Success)
                    {
                        if (match.Groups[2].Success && !match.Groups[3].Success)
                            tokens.Add(new LexerToken(LexerTokenValue.INT, int.Parse(match.Value), line, index));
                        else
                            tokens.Add(new LexerToken(LexerTokenValue.FLOAT, float.Parse(match.Value), line, index));
                        index += match.Length - 1;
                        continue;
                    }

                    LexerTokenValue token = GetOperator(ref rawLine, ref index);
                    if (token == LexerTokenValue.UNKNOWN)
                        RaiseError(file, line, index, $"Lexer cannot decipher token '{rawLine[index]}'");
                    if (token == LexerTokenValue.ENDL && tokens[tokens.Count - 1].Type == LexerTokenValue.ENDL)
                        continue;
                    tokens.Add(new LexerToken(token, subString[0], line, index));

                }

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

                case "var":
                    return LexerTokenValue.VAR;

                case "struct":
                    return LexerTokenValue.STRUCT;
                case "func":
                    return LexerTokenValue.FUNC;

                case "if":
                    return LexerTokenValue.IF;
                case "elif":
                    return LexerTokenValue.ELIF;
                case "else":
                    return LexerTokenValue.ELSE;

                case "for":
                    return LexerTokenValue.FOR;
                case "while":
                    return LexerTokenValue.WHILE;
                case "switch":
                    return LexerTokenValue.SWITCH;
                case "default":
                    return LexerTokenValue.DEFAULT;

                case "using":
                    return LexerTokenValue.USING;
                case "as":
                    return LexerTokenValue.USING;
                case "parent":
                    return LexerTokenValue.PARENT;

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
                    return LexerTokenValue.ENDL;
                case ',':
                    return LexerTokenValue.COMMA;
                case ':':
                    return LexerTokenValue.SEMICOLON;
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
