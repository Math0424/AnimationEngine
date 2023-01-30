using AnimationEngine.Language;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;

namespace AnimationEngine.LanguageV1
{
    internal class Parser
    {
        private ScriptV1Generator compiler;
        private List<Token> tokens { get { return compiler.Tokens; } }

        private List<Token> GrabUntil(int start, TokenType stop, bool filterEndL = false)
        {
            List<Token> arr = new List<Token>();
            for (int i = start; i < tokens.Count; i++)
            {
                if (filterEndL && tokens[i].Type == TokenType.ENDL)
                {
                    continue;
                }
                arr.Add(tokens[i]);
                if (tokens[i].Type == stop)
                {
                    break;
                }
            }
            return arr;
        }

        private int FindNext(int start, TokenType type)
        {
            for (int i = start; i < tokens.Count; i++)
            {
                if (tokens[i].Type == type)
                {
                    return i;
                }
            }
            return -1;
        }

        public Parser(ScriptV1Generator compiler)
        {
            this.compiler = compiler;

            //protected object name
            compiler.objects.Add("block", new Entity());

            AssembleMathVectors(); // [1, 2, 3] -> MVector
            AssembleArguments(); // Keyword ( ... ) { ... } -> Keyword Vector { ... }
            AssembleBodys(0); // Keyword Vector { ... } -> Keyword Vector Vector

            ParseHeaders();
            ParseObjects();
            ParseFunctions();
            ParseActions();
        }

        private void AssembleMathVectors()
        {
            int next = FindNext(0, TokenType.LSQBRC);
            while (next != -1)
            {
                if (FindNext(next, TokenType.RSQBRC) == -1)
                {
                    throw compiler.DetailedLog($"Missing closing bracket", tokens[next]);
                }
                
                int begin = next++;
                Token[] vec = AssembleVector(compiler.Tokens.ToArray(), ref next, TokenType.RSQBRC);

                if (vec.Length != 3)
                {
                    throw compiler.DetailedLog("Math vectors must be length 3", compiler.Tokens[next]);
                }

                foreach(var x in vec)
                {
                    if (x.Type != TokenType.INT && x.Type != TokenType.FLOAT)
                    {
                        throw compiler.DetailedLog($"Value must be a number, found {x.Type}", x);
                    }
                }

                compiler.Tokens.RemoveRange(begin, next - begin);
                compiler.Tokens[begin] = new Token(TokenType.MVECTOR, vec.ToVector3(), compiler.Tokens[begin].Line, compiler.Tokens[begin].Col);
                next -= next - begin;

                next = FindNext(next, TokenType.LSQBRC);
            }
        }

        private void AssembleArguments()
        {

        }

        private void AssembleBodys(int start)
        {
            if (start > compiler.Tokens.Count)
                return;

            start = FindNext(start, TokenType.LBRACE);

            if (start == -1)
                return;

            int end = FindNext(start, TokenType.RBRACE);
            if (end == -1)
                throw compiler.DetailedLog("Missing closing bracket", compiler.Tokens[start]);

            if (end > FindNext(start, TokenType.LBRACE))
            {
                AssembleBodys(start + 1);
            }

            end = FindNext(start, TokenType.RBRACE);
            if (end == -1)
                throw compiler.DetailedLog("Missing closing bracket", compiler.Tokens[start]);


            List<Token> args = new List<Token>();
            for (int i = start + 1; i < end; i++)
            {
                args.Add(tokens[i]);
            }
            tokens.RemoveRange(start, end - start);
            tokens[start] = new Token(TokenType.VECTOR, args.ToArray(), tokens[start].Line, tokens[start].Col);

            AssembleBodys(++start);
        }




        private Token[] AssembleVector(Token[] arr, ref int index, TokenType stop)
        {
            if (arr[index].Type == stop)
            {
                return new Token[0];
            }

            List<Token> args = new List<Token>(); 
            bool comma = true;
            while (index < arr.Length)
            {
                if (comma)
                {
                    if (arr[index].Type == TokenType.COM)
                    {
                        throw compiler.DetailedLog($"Too many seperators", arr[index]);
                    }
                    args.Add(arr[index]);
                    comma = false;
                }
                else if (arr[index].Type == stop)
                {
                    if (comma)
                    {
                        throw compiler.DetailedLog($"Invalid seperator position", arr[index - 1]);
                    }
                    break;
                }
                else if (arr[index].Type == TokenType.COM)
                {
                    comma = true;
                }
                index++;
            }
            return args.ToArray();
        }

        private V1Expression ParseExpression(ref int index, ref Token[] toks)
        {
            if (toks[index].Type != TokenType.DOT) {
                throw compiler.DetailedLog($"Invalid expression seperator", toks[index]);
            } else if (toks[index + 1].Type != TokenType.KEWRD) {
                throw compiler.DetailedLog($"Invalid expression keyword", toks[index + 1]);
            } else if (toks[index + 2].Type != TokenType.LPAREN) {
                throw compiler.DetailedLog($"Invalid expression opening", toks[index + 2]);
            }
            V1Expression exp = new V1Expression()
            {
                Title = toks[index + 1]
            };
            exp.Title.Value = exp.Title.Value.ToString().ToLower();
            index += 3;

            exp.Args = AssembleVector(toks, ref index, TokenType.RPAREN);
            index++;
            
            return exp;
        }

        private V1Call[] ParseBody(ref Token[] toks)
        {
            List<V1Call> calls = new List<V1Call>();
            
            int next = 0;
            while (next < toks.Length)
            {
                if (toks[next].Type == TokenType.KEWRD)
                {
                    V1Call call = new V1Call()
                    {
                        Title = toks[next],
                    };
                    call.Title.Value = call.Title.Value.ToString().ToLower();

                    if (toks[next + 1].Type == TokenType.DOT)
                    {
                        call.Type = TokenType.OBJECT;
                        List<V1Expression> exprs = new List<V1Expression>();
                        next++;
                        while (next < toks.Length && toks[next].Type != TokenType.ENDL)
                        {
                            exprs.Add(ParseExpression(ref next, ref toks));
                        }
                        
                        call.Expressions = exprs.ToArray();
                    }
                    else if (toks[next + 1].Type != TokenType.LPAREN && toks[next + 2].Type != TokenType.RPAREN)
                    {
                        throw compiler.DetailedLog($"Invalid function call", toks[next]);
                    } 
                    else
                    {
                        call.Type = TokenType.FUNCCALL;
                    }

                    calls.Add(call);
                }
                next++;
            }
            return calls.ToArray();
        }

        private void ParseActions()
        {
            int next = FindNext(0, TokenType.ACTION);
            while (next != -1)
            {
                next++;
                if (tokens[next].Type != TokenType.KEWRD)
                {
                    throw compiler.DetailedLog($"Invalid action format", tokens[next]);
                }

                if (tokens[next + 1].Type != TokenType.LPAREN)
                {
                    throw compiler.DetailedLog($"Missing opening '('", tokens[next]);
                }

                V1ScriptAction act = new V1ScriptAction()
                {
                    Name = tokens[next],
                };
                act.Name.Value = act.Name.Value.ToString().ToLower();

                next += 2;
                act.Paramaters = AssembleVector(tokens.ToArray(), ref next, TokenType.RPAREN);

                if (tokens[next + 1].Type != TokenType.VECTOR)
                {
                    throw compiler.DetailedLog($"Missing action body", tokens[next]);
                }

                List<V1Function> functions = new List<V1Function>();
                Token[] inner = (Token[])tokens[next + 1].Value;
                int curr = 0;
                while (curr < inner.Length)
                {
                    if (inner[curr].Type == TokenType.KEWRD)
                    {
                        V1Function func = new V1Function()
                        {
                            Name = inner[curr],
                        };
                        func.Name.Value = func.Name.Value.ToString().ToLower();

                        if (inner[++curr].Type != TokenType.LPAREN)
                        {
                            throw compiler.DetailedLog($"Missing opening '('", inner[curr]);
                        }

                        curr++;
                        func.Paramaters = AssembleVector(inner, ref curr, TokenType.RPAREN);
                        curr++;

                        if (inner[curr].Type != TokenType.VECTOR)
                        {
                            throw compiler.DetailedLog("Missing body for action statement", inner[curr]);
                        }

                        Token[] arr = (Token[])inner[curr].Value;
                        func.Body = ParseBody(ref arr);

                        functions.Add(func);
                    }
                    curr++;
                }

                act.Funcs = functions.ToArray();
                compiler.actions.Add(act);
                next = FindNext(next, TokenType.ACTION);
            }
        }

        private void ParseFunctions()
        {
            int next = FindNext(0, TokenType.FUNC);
            while(next != -1)
            {
                if (next + 4 > tokens.Count)
                {
                    throw compiler.DetailedLog("Unable to parse function", tokens[next]);
                }

                if (tokens[next + 1].Type != TokenType.KEWRD || tokens[next + 2].Type != TokenType.LPAREN || 
                    tokens[next + 3].Type != TokenType.RPAREN || tokens[next + 4].Type != TokenType.VECTOR)
                {
                    throw compiler.DetailedLog($"Invalid function format", tokens[next + 1]);
                }

                string name = tokens[next + 1].Value.ToString().ToLower();
                if (compiler.functions.ContainsKey(name))
                {
                    throw compiler.DetailedLog($"Func has multiple declarations", tokens[next + 1]);
                }

                Token[] arr = (Token[])tokens[next + 4].Value;
                V1Function func = new V1Function()
                {
                    Name = tokens[next + 1],
                    Body = ParseBody(ref arr),
                };
                func.Name.Value = func.Name.Value.ToString().ToLower();
                compiler.functions.Add(name, func);

                next = FindNext(next + 1, TokenType.FUNC);
            }
        }

        private void ParseObjects()
        {
            int next = FindNext(0, TokenType.USING);
            List<Token> arr;
            while (next != -1)
            {
                arr = GrabUntil(next, TokenType.ENDL);

                string name = arr[1].Value.ToString().ToLower();
                if (arr.Count <= 4) {
                    throw compiler.DetailedLog($"Invalid object format!", arr[0]);
                } else if (arr[2].Type != TokenType.AS) {
                    throw compiler.DetailedLog($"Invalid object format!", arr[2]);
                } else if (compiler.objects.ContainsKey(name)) {
                    throw compiler.DetailedLog($"Duplicate object declaration", arr[1]);
                }

                Entity obj = new Entity()
                {
                    Name = arr[1],
                    Type = arr[3],
                };

                if (arr.Count >= 8)
                {
                    if (arr[4].Type != TokenType.LPAREN)
                    {
                        throw compiler.DetailedLog($"Missing parentheses", arr[4]);
                    }
                    if (arr[arr.Count - 2].Type != TokenType.RPAREN)
                    {
                        throw compiler.DetailedLog($"Missing parentheses", arr[arr.Count - 2]);
                    }

                    obj.Args = new Token[arr.Count - 7];
                    Array.Copy(arr.ToArray(), 5, obj.Args, 0, arr.Count - 7);
                }

                compiler.objects.Add(name, obj);
                next = FindNext(next + 1, TokenType.USING);
            }
        }

        private void ParseHeaders()
        {
            int next = FindNext(0, TokenType.AT);
            List<Token> arr;
            while (next != -1)
            {
                arr = GrabUntil(next, TokenType.ENDL);
                if (arr.Count == 4)
                {
                    string name = arr[1].Value.ToString().ToLower();
                    if (compiler.headers.ContainsKey(name))
                    {
                        throw compiler.DetailedLog($"Duplicate header", arr[1]);
                    }
                    compiler.headers.Add(name, arr[2]);
                }
                next = FindNext(next + 1, TokenType.AT);
            }
        }

    }
}
