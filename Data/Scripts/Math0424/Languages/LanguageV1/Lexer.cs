using System;

namespace AnimationEngine.LanguageV1
{
    internal static class Lexer
    {
        public static void TokenizeScript(Script compiler)
        {
            for (int lineNum = 0; lineNum < compiler.RawScript.Length; lineNum++)
            {
                string line = compiler.RawScript[lineNum];

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                int index = -1;
                while (index < line.Length - 1)
                {
                    index++;
                    char c = line[index];
                    if (char.IsWhiteSpace(c)) { continue; };
                    if (c == '#') break;

                    //keywords
                    if (char.IsLetter(c) || c == '_')
                    {
                        object obj;
                        TokenType type = GetToken(GetWord(ref line, ref index), out obj);
                        compiler.Tokens.Add(new Token(type, obj, lineNum, index));
                        continue;
                    }

                    //numbers
                    if (char.IsDigit(c) || c == '-')
                    {
                        TokenType type;
                        object val = GetNum(ref line, ref index, out type);
                        if (val != null)
                        {
                            compiler.Tokens.Add(new Token(type, val, lineNum, index));
                            continue;
                        }
                        throw compiler.Error.AppendError("Error parsing number", line, index);
                    }

                    //strings
                    if (c == '\"' || c == '\'')
                    {
                        string value = GetString(ref line, ref index);
                        if (value.Length == 0)
                        {
                            throw compiler.Error.AppendError("Error parsing string", line, index);
                        }
                        compiler.Tokens.Add(new Token(TokenType.STR, value, lineNum, index));
                        continue;
                    }

                    //operators
                    TokenType token = GetOperator(ref line, ref index);
                    if (token == TokenType.UKWN)
                    {
                        throw compiler.Error.AppendError($"Unknown token!", line, index);
                    }
                    if (token == TokenType.ENDL && compiler.Tokens[compiler.Tokens.Count - 1].Type == TokenType.ENDL)
                    {
                        continue;
                    }
                    compiler.Tokens.Add(new Token(token, char.ToString(line[index]), lineNum, index));
                }

                //add endl if end of line
                if (compiler.Tokens[compiler.Tokens.Count - 1].Type != TokenType.ENDL)
                {
                    compiler.Tokens.Add(new Token(TokenType.ENDL, "", lineNum, index));
                }
            }
        }

        private static TokenType GetOperator(ref string line, ref int index)
        {
            switch (line[index])
            {
                case ';':
                    return TokenType.ENDL;
                case ',':
                    return TokenType.COM;
                case ':':
                    return TokenType.COL;
                case '.':
                    return TokenType.DOT;
                case '@':
                    return TokenType.AT;
                case '{':
                    return TokenType.LBRACE;
                case '}':
                    return TokenType.RBRACE;
                case '(':
                    return TokenType.LPAREN;
                case ')':
                    return TokenType.RPAREN;
                case '[':
                    return TokenType.LSQBRC;
                case ']':
                    return TokenType.RSQBRC;

                case '=':
                    return TokenType.EQL;  
                case '+':
                    return TokenType.ADD;  
                case '-':
                    return TokenType.SUB;  
                case '*':
                    return TokenType.MUL;  
                case '/':
                    return TokenType.DIV;  
                case '<':
                    return TokenType.GRT;  
                case '>':
                    return TokenType.LST;  
                case '!':
                    return TokenType.NOT;  
                case '&':
                    return TokenType.AND;  
                case '|':
                    return TokenType.OR;
                case '%':
                    return TokenType.MOD;

            }
            return TokenType.UKWN;
        }

        private static TokenType GetToken(string word, out object obj)
        {
            obj = word;

            string lword = word.ToLower();
            if (lword.Equals("as")) { return TokenType.AS; }

            else if (lword.Equals("using")) { return TokenType.USING; }
            else if (lword.Equals("as")) { return TokenType.AS; }
            else if (lword.Equals("func")) { return TokenType.FUNC; }
            else if (lword.Equals("action")) { return TokenType.ACTION; }
            else if (lword.Equals("var")) { return TokenType.VAR; }

            else if (lword.Equals("true")) {
                obj = true;
                return TokenType.BOOL; 
            }
            else if (lword.Equals("false")) {
                obj = false;
                return TokenType.BOOL; 
            }

            else if (lword.Equals("subpart")) { return TokenType.SUBPART; }
            else if (lword.Equals("button")) { return TokenType.BUTTON; }
            else if (lword.Equals("basicik")) { return TokenType.BASICIK; }
            else if (lword.Equals("light")) { return TokenType.LIGHT; }
            else if (lword.Equals("emitter")) { return TokenType.EMITTER; }
            else if (lword.Equals("emissive")) { return TokenType.EMISSIVE; }
            else if (lword.Equals("parent")) { return TokenType.PARENT; }

            else if (lword.Equals("!=")) { return TokenType.NOTEQ; }
            else if (lword.Equals("==")) { return TokenType.COMP; }
            else if (lword.Equals("<=")) { return TokenType.GRTE; }
            else if (lword.Equals(">=")) { return TokenType.LSTE; }
            else if (lword.Equals("if")) { return TokenType.IF; }
            else if (lword.Equals("elif")) { return TokenType.ELIF; }
            else if (lword.Equals("else")) { return TokenType.ELSE; }

            foreach (var x in Enum.GetValues(typeof(ShortHandLerp)))
            {
                if(x.ToString().ToLower().Equals(lword))
                {
                    obj = x;
                    return TokenType.LERP;
                }
            }

            return TokenType.KEWRD;
        }

        private static string GetWord(ref string line, ref int index)
        {
            string value = "";
            while (index < line.Length)
            {
                char c = line[index];
                if (char.IsLetter(c) || char.IsDigit(c) || c == '_')
                {
                    value += c;
                }
                else
                {
                    break;
                }
                index++;
            }
            index--;
            return value;
        }

        private static string GetString(ref string line, ref int index)
        {
            string value = "";
            while (index < line.Length)
            {
                char c = line[++index];
                if (c == '\'' || c == '\"')
                {
                    return value;
                }
                value += c;
            }
            return null;
        }

        private static object GetNum(ref string line, ref int index, out TokenType type)
        {
            type = TokenType.INT;
            string value = "";
            if (line[index] == '-')
            {
                if ((index + 1 < line.Length) ? char.IsDigit(line[index + 1]) : false)
                {
                    value += line[index++];
                }
                else
                {
                    return null;
                }
            }

            bool hadSeperator = false;
            while (index < line.Length)
            {
                char c = line[index];
                char cn = (index + 1 < line.Length) ? line[index + 1] : 'x';
                if (c == '.')
                {
                    if (!hadSeperator)
                    {
                        type = TokenType.FLOAT;
                        hadSeperator = true;
                        if (!char.IsDigit(cn))
                        {
                            break;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (!char.IsDigit(c))
                {
                    break;
                }
                value += c;
                index++;
            }
            index--;
            
            if (type == TokenType.FLOAT)
            {
                return float.Parse(value);
            } 
            return int.Parse(value);
        }

    }
}
