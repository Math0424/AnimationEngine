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
                    case Lexer.LexerTokenValue.SEMICOLON:
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

                        headers.Reverse();
                        foreach (var x in headers)
                            node.Children.Insert(0, x);

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
            Lexer.LexerToken variableName;
            if (arr[index + 1].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Not a valid variable name at {arr[index + 1]}");
            variableName = arr[index + 1];

            Lexer.LexerTokenValue value = Lexer.LexerTokenValue.DEFAULT;
            if (arr[index + 2].Type == Lexer.LexerTokenValue.COLON)
            {
                if (!arr[index + 3].Type.IsVariable())
                    throw new Exception($"Cannot use non variable type for variable {arr[index + 3]}");
                value = arr[index + 3].Type;
                index += 2;
            }

            if (arr[index + 2].Type != Lexer.LexerTokenValue.EQUAL)
                throw new Exception($"Not a valid variable declaration at {arr[index + 2]}");

            if (!arr[index + 3].Type.IsLiteralVariable())
                throw new Exception($"Cannot initalize variable to non variable type {arr[index + 3]}");

            if (value != Lexer.LexerTokenValue.DEFAULT && !value.IsLiteralMatch(arr[index + 3].Type))
                throw new Exception($"Variable type missmatch {value} is not of type {arr[index + 3].Type}");

            value = arr[index + 3].Type;

            ASTNode variable = new ASTNode(Lexer.LexerTokenValue.VARIABLE, variableName.RawValue);
            variable.Children.Add(new ASTNode(value, arr[index + 3]));
            index += 4;
            return variable;
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

            function.Children.Add(ParseBody(arr, ref index));
            return function;
        }

        private static ASTNode IfStatement(Lexer.LexerToken[] arr, ref int index)
        {
            index++;
            ASTNode ifNode = new ASTNode(Lexer.LexerTokenValue.IF);
            
            ifNode.Children.Add(ParseExpression(arr, ref index));
            ifNode.Children.Add(ParseBody(arr, ref index));

            if (arr[index].Type == Lexer.LexerTokenValue.ELSE)
            {
                index++;
                if (arr[index].Type == Lexer.LexerTokenValue.IF)
                    ifNode.Children.Add(IfStatement(arr, ref index));
                else
                    ifNode.Children.Add(ParseBody(arr, ref index));
            } 

            return ifNode;
        }

        private static ASTNode WhileStatement(Lexer.LexerToken[] arr, ref int index)
        {
            index++;
            ASTNode ifNode = new ASTNode(Lexer.LexerTokenValue.WHILE);
            ifNode.Children.Add(ParseExpression(arr, ref index));
            ifNode.Children.Add(ParseBody(arr, ref index));
            return ifNode;
        }

        private static ASTNode ParseExpression(Lexer.LexerToken[] arr, ref int index, int precedence = 0)
        {
            int newIndex = index;
            ASTNode left = ParsePrimaryExpression(arr, ref newIndex);
            index = newIndex;

            while (index < arr.Length && arr[index].Type.GetPrecedence() > precedence)
            {
                Lexer.LexerToken operatorToken = arr[index];
                int operatorPrecedence = operatorToken.Type.GetPrecedence();
                index++;

                ASTNode right = ParseExpression(arr, ref index, operatorPrecedence);

                ASTNode operatorNode = new ASTNode(operatorToken);
                operatorNode.Children.Add(left);
                operatorNode.Children.Add(right);
                left = operatorNode;
            }

            return left;
        }

        private static ASTNode ParsePrimaryExpression(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index].Type == Lexer.LexerTokenValue.RPAREN)
                throw new Exception($"Empty parenthesis at {arr[index]}");

            if (arr[index].Type == Lexer.LexerTokenValue.LPAREN)
            {
                index++;
                ASTNode expression = ParseExpression(arr, ref index, 0);
                if (arr[index].Type != Lexer.LexerTokenValue.RPAREN)
                    throw new Exception($"Expected closing parenthesis at {arr[index]}");
                index++;
                return expression;
            }
            else if (arr[index].Type == Lexer.LexerTokenValue.NOT)
            {
                index++;
                ASTNode operand = ParsePrimaryExpression(arr, ref index);
                ASTNode notNode = new ASTNode(Lexer.LexerTokenValue.NOT);
                notNode.Children.Add(operand);
                return notNode;
            }
            else if (arr[index].Type.IsLiteralMathVariable() || arr[index].Type == Lexer.LexerTokenValue.KEYWORD)
            {
                ASTNode node = new ASTNode(arr[index]);
                index++;
                return node;
            }
            else
                throw new Exception($"Unexpected token in expression {arr[index]}");
        }

        private static ASTNode ParseBody(Lexer.LexerToken[] arr, ref int index)
        {
            ASTNode bodyNode = new ASTNode(Lexer.LexerTokenValue.BODY);
            if (!StrictNext(Lexer.LexerTokenValue.LBRACE, arr, ref index)) // {
                throw new Exception($"Cannot find body start for {arr[index]}");
            index++;

            while (!StrictNext(Lexer.LexerTokenValue.RBRACE, arr, ref index)) // }
            {
                switch (arr[index].Type)
                {
                    case Lexer.LexerTokenValue.LET:
                        bodyNode.Children.Add(Variable(arr, ref index));
                        break;
                    case Lexer.LexerTokenValue.IF:
                        bodyNode.Children.Add(IfStatement(arr, ref index));
                        break;
                    case Lexer.LexerTokenValue.WHILE:
                        bodyNode.Children.Add(WhileStatement(arr, ref index));
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

                    case Lexer.LexerTokenValue.SEMICOLON:
                        index++;
                        break;
                    default:
                        throw new Exception($"Invalid token found in body domain\n{arr[index]}");
                }
            }
            index++;
            return bodyNode;
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

                if (arr[++index].Type != Lexer.LexerTokenValue.SEMICOLON)
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
