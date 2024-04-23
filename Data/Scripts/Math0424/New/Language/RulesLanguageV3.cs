using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    internal class RulesLanguageV3 : RulesHelper
    {
        public static ASTNode Program(Lexer.LexerToken[] arr, ref int index)
        {
            ASTNode root = new ASTNode(Lexer.LexerTokenValue.ROOT);
            while(index < arr.Length)
            {
                switch (arr[index].Type)
                {
                    case Lexer.LexerTokenValue.ENDL:
                        index++;
                        break;

                    case Lexer.LexerTokenValue.LSQBRC:
                        List<ASTNode> headers = new List<ASTNode>();
                        ASTNode headerNode;
                        while (Header(arr, ref index, out headerNode))
                            headers.Add(headerNode);

                        ASTNode node;
                        switch(arr[index].Type)
                        {
                            case Lexer.LexerTokenValue.LET:
                                node = Variable(arr, ref index);
                                break;
                            case Lexer.LexerTokenValue.FUNC:
                                node = Function(arr, ref index);
                                break;
                            default:
                                throw new Exception($"Headers must be above a function or a globalVariable at {arr[index]}");
                        }

                        foreach (var x in headers)
                            node.Children.Add(x);

                        root.Children.Add(node);
                        break;

                    case Lexer.LexerTokenValue.LET:
                        root.Children.Add(Variable(arr, ref index));
                        break;

                    case Lexer.LexerTokenValue.FUNC:
                        root.Children.Add(Function(arr, ref index));
                        break;

                        // TODO: handle imports
                    case Lexer.LexerTokenValue.AT:
                        EndLine(arr, ref index);
                        break;
                    
                    default:
                        throw new Exception($"Invalid token found in script domain\n{arr[index]}");
                }
            }
            return root;
        }

        private static ASTNode Variable(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index + 1].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Not a valid variable declaration at {arr[index + 1]}");

            if (arr[index + 2].Type != Lexer.LexerTokenValue.EQUAL)
                throw new Exception($"Not a valid variable declaration at {arr[index + 2]}");
            
            switch (arr[index + 3].Type)
            {
                case Lexer.LexerTokenValue.LET:
                    break;
                default:
                    throw new Exception($"Invalid global variable type at {arr[index + 3]}");
            }

            ASTNode type = new ASTNode(Lexer.LexerTokenValue.VARIABLE, new object[] { arr[index].Type, arr[index + 1], arr[index + 3] });
            index += 4;
            return type;
        }

        private static ASTNode Function(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.FUNC)
                throw new Exception($"Not a function at {arr[index]}");

            ASTNode function = new ASTNode(Lexer.LexerTokenValue.FUNC, new object[2]);
            ((object[])function.Value)[0] = arr[++index].RawValue;

            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Method name must be a keyword at {arr[index]}");

            index++;
            int args = ParseParenthesesSimple(arr, ref index, ref function); // (va, l, ue)
            ((object[])function.Value)[1] = args;

            ParseTillFunctionExit(function, arr, ref index);
            return function;
        }

        private static void ParseTillFunctionExit(ASTNode parent, Lexer.LexerToken[] arr, ref int index)
        {
            if (!StrictNext(Lexer.LexerTokenValue.LBRACE, arr, ref index)) // {
                throw new Exception($"Cannot find function start for {parent.Token}");
            index++;

            while (!StrictNext(Lexer.LexerTokenValue.RBRACE, arr, ref index)) // }
            {
                switch (arr[index].Type)
                {
                    case Lexer.LexerTokenValue.VAR:
                    case Lexer.LexerTokenValue.STRING:
                    case Lexer.LexerTokenValue.BOOL:
                    case Lexer.LexerTokenValue.INT:
                    case Lexer.LexerTokenValue.FLOAT:
                        parent.Children.Add(Variable(arr, ref index));
                        break;

                    case Lexer.LexerTokenValue.IF:
                        break;
                    case Lexer.LexerTokenValue.WHILE:
                        break;
                    case Lexer.LexerTokenValue.SWITCH:
                        break;
                    case Lexer.LexerTokenValue.RETURN:
                        break;

                    // parse math, functions, ect
                    // keyword[value] = 10
                    // FunctionCall(va, lue)
                    // value = 10
                    case Lexer.LexerTokenValue.KEYWORD:
                        break;

                    case Lexer.LexerTokenValue.ENDL:
                        index++;
                        break;
                    default:
                        throw new Exception($"Invalid token found in function domain\n{arr[index]}");
                }
            }
            index++;
        }

        private static bool Header(Lexer.LexerToken[] arr, ref int index, out ASTNode node)
        {
            node = default(ASTNode);

            if (arr[index].Type == Lexer.LexerTokenValue.LSQBRC)
            {
                Lexer.LexerToken headerName = arr[++index];
                if (headerName.Type != Lexer.LexerTokenValue.KEYWORD)
                    throw new Exception($"Cannot parse function header at {headerName}");

                node = new ASTNode(Lexer.LexerTokenValue.HEADER, headerName.RawValue);
                if (arr[++index].Type != Lexer.LexerTokenValue.RSQBRC)
                    ParseParenthesesSimple(arr, ref index, ref node);
                if (arr[index].Type != Lexer.LexerTokenValue.RSQBRC)
                    throw new Exception($"Invalid Function header format at {arr[index]}");

                if (arr[++index].Type != Lexer.LexerTokenValue.ENDL)
                    throw new Exception($"Function header must end at {arr[index]}");
                index++;

                return true;
            }
            return false;
        }

        private static int ParseParenthesesSimple(Lexer.LexerToken[] arr, ref int index, ref ASTNode node)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.LPAREN)
                throw new Exception($"Cannot read parentheses at {arr[index]}");

            int args = 0;
            bool comma = false;
            while(arr[++index].Type != Lexer.LexerTokenValue.RPAREN)
            {
                if (comma && arr[index].Type != Lexer.LexerTokenValue.COMMA)
                    throw new Exception($"Missing seperator at {arr[index]}");
                else if (arr[index].Type == Lexer.LexerTokenValue.RPAREN)
                    break;
                else if(!comma)
                {
                    args++;
                    node.Children.Add(new ASTNode(arr[index]));
                }
                comma = !comma;
            }
            index++;
            return args;
        }

    }
}
