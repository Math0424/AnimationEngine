using System;
using System.Collections.Generic;

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
                        while (MethodHeader(arr, ref index, out headerNode))
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

                    case Lexer.LexerTokenValue.STRUCT:
                        root.Children.Add(ParseStruct(arr, ref index));
                        break;

                    case Lexer.LexerTokenValue.AT:
                        EndLine(arr, ref index);
                        break;

                    case Lexer.LexerTokenValue.IMPORT:
                        root.Children.Add(Import(arr, ref index));
                        break;

                    case Lexer.LexerTokenValue.USING:
                        root.Children.Add(ParseUsing(arr, ref index));
                        break;

                    default:
                        throw new Exception($"Invalid token found in script domain\n{arr[index]}");
                }
            }
            return root;
        }

        private static ASTNode ParseUsing(Lexer.LexerToken[] arr, ref int index)
        {
            index++;
            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Using statement requires a name {arr[index]}");
            var name = arr[index];

            index++;
            if (arr[index].Type != Lexer.LexerTokenValue.AS)
                throw new Exception($"Using statement requires an as statement {arr[index]}");
            index++;

            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Using statement requires a type {arr[index]}");
            var type = arr[index];

            index++;
            var usingNode = new ASTNode(Lexer.LexerTokenValue.USING, name.RawValue);
            var keywordType = new ASTNode(type);
            ParseParenthesesSimple(arr, ref index, ref keywordType);
            usingNode.Children.Add(keywordType);

            if (arr[index].Type == Lexer.LexerTokenValue.PARENT)
            {
                index++;
                if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                    throw new Exception($"parent statement requires a name {arr[index]}");
                usingNode.Children.Add(new ASTNode(Lexer.LexerTokenValue.PARENT, arr[index].RawValue));
                index++;
            }

            return usingNode;
        }

        private static ASTNode Import(Lexer.LexerToken[] arr, ref int index)
        {
            index++;
            if (arr[index].Type != Lexer.LexerTokenValue.LSTRING)
                throw new Exception($"Must use a string on an import statement {arr[index]}");
            return new ASTNode(Lexer.LexerTokenValue.IMPORT, arr[index++].RawValue);
        }

        private static ASTNode ParseStruct(Lexer.LexerToken[] arr, ref int index)
        {
            Lexer.LexerToken structName;
            index++;
            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Not a valid struct name at {arr[index]}");
            structName = arr[index];
            
            index++;
            if (!StrictNext(arr, ref index, Lexer.LexerTokenValue.LBRACE)) // {
                throw new Exception($"Cannot find body start for struct {structName}");

            ASTNode customStruct = new ASTNode(Lexer.LexerTokenValue.STRUCT, structName.RawValue);
            while (!ExpectNext(arr, index, Lexer.LexerTokenValue.RBRACE))
            {
                index++;
                if (arr[index].Type == Lexer.LexerTokenValue.SEMICOLON)
                    continue;

                if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                    throw new Exception($"Not a valid variable name at {arr[index]}");
                Lexer.LexerToken variableName = arr[index];

                index++;
                if (arr[index].Type != Lexer.LexerTokenValue.COLON)
                    throw new Exception($"Must have semicolon for variable {arr[index - 1]}");

                index++;
                if (!arr[index].Type.IsVariable())
                    throw new Exception($"Variable must be a variable type {arr[index - 2]}");
                Lexer.LexerToken variableType = arr[index];

                ASTNode variable = new ASTNode(Lexer.LexerTokenValue.VARIABLE, variableName.RawValue);
                variable.Children.Add(new ASTNode(variableType));
                customStruct.Children.Add(variable);
            }
            index += 2;

            //variable.Children.Add(new ASTNode(value, arr[index + 3]));
            return customStruct;
        }

        private static ASTNode Variable(Lexer.LexerToken[] arr, ref int index)
        {
            index++;
            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Not a valid variable name at {arr[index]}");
            Lexer.LexerToken variableName = arr[index];

            Lexer.LexerToken? variableToken = null;
            if (arr[index + 1].Type == Lexer.LexerTokenValue.COLON)
            {
                variableToken = arr[index + 2];
                index += 2;
            }

            index++;
            if (arr[index].Type != Lexer.LexerTokenValue.EQUAL)
                throw new Exception($"Not a valid variable declaration at {arr[index]}");
            index++;

            ASTNode variable = new ASTNode(variableToken.HasValue ? Lexer.LexerTokenValue.VARIABLE : Lexer.LexerTokenValue.UNKNOWN_VARIABLE, variableName.RawValue);
            if (variableToken.HasValue)
                variable.Children.Add(new ASTNode(variableToken.Value));

            if (arr[index].Type == Lexer.LexerTokenValue.DEFAULT)
            {
                variable.Children.Add(new ASTNode(arr[index]));
                index++;
            }
            else if (arr[index].Type.IsLiteralVariable() && arr[index + 1].Type == Lexer.LexerTokenValue.SEMICOLON)
            {
                if (variableToken.HasValue && !variableToken.Value.Type.IsLiteralMatch(arr[index].Type))
                    throw new Exception($"Missmatch of literal and initalized variable at {arr[index]}");
                
                variable.Children.Add(new ASTNode(arr[index]));
                index++;
            } 
            else
            {
                variable.Children.Add(ParseExpression(arr, ref index));
            }
            index++;
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
            int args = ParseVariablesSimple(arr, ref index, ref function); // (va, l, ue)
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
            ASTNode left = ParseExpressionHelper(arr, ref newIndex);
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

        private static ASTNode ParseExpressionHelper(Lexer.LexerToken[] arr, ref int index)
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
                ASTNode operand = ParseExpressionHelper(arr, ref index);
                ASTNode notNode = new ASTNode(Lexer.LexerTokenValue.NOT);
                notNode.Children.Add(operand);
                return notNode;
            }
            else if (arr[index].Type.IsLiteralMathVariable())
            {
                ASTNode node = new ASTNode(arr[index]);
                index++;
                return node;
            }
            else if(arr[index].Type == Lexer.LexerTokenValue.KEYWORD)
            {
                if (arr[index + 1].Type == Lexer.LexerTokenValue.COLON || arr[index + 1].Type == Lexer.LexerTokenValue.LPAREN)
                    return ParseCall(arr, ref index);
                else
                {
                    ASTNode node = new ASTNode(arr[index]);
                    index++;
                    return node;
                }
            }
            else
                throw new Exception($"Unexpected token in expression {arr[index]}");
        }

        private static ASTNode Return(Lexer.LexerToken[] arr, ref int index)
        {
            index++;
            ASTNode returnNode = new ASTNode(Lexer.LexerTokenValue.RETURN);
            if (arr[index].Type == Lexer.LexerTokenValue.SEMICOLON)
                return returnNode;
            
            returnNode.Children.Add(ParseExpression(arr, ref index));
            return returnNode;
        }

        private static ASTNode ParseBody(Lexer.LexerToken[] arr, ref int index)
        {
            ASTNode bodyNode = new ASTNode(Lexer.LexerTokenValue.BODY);
            if (!StrictNext(arr, ref index, Lexer.LexerTokenValue.LBRACE)) // {
                throw new Exception($"Cannot find body start for {arr[index]}");
            index++;

            while (!StrictNext(arr, ref index, Lexer.LexerTokenValue.RBRACE)) // }
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
                    case Lexer.LexerTokenValue.BREAK:
                        bodyNode.Children.Add(new ASTNode(arr[index]));
                        index++;
                        break;
                    case Lexer.LexerTokenValue.RETURN:
                        bodyNode.Children.Add(Return(arr, ref index));
                        break;

                    // parse math, functions, ect
                    // keyword[value] = 10
                    // FunctionCall(va, lue)
                    // value = 10
                    case Lexer.LexerTokenValue.KEYWORD:
                        bodyNode.Children.Add(Keyword(arr, ref index));
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

        private static ASTNode Keyword(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index + 1].Type == Lexer.LexerTokenValue.EQUAL)
            {
                var equal = new ASTNode(Lexer.LexerTokenValue.EQUAL, arr[index].RawValue);
                index += 2;
                equal.Children.Add(ParseExpression(arr, ref index));
                return equal;
            }
            else if(arr[index + 1].Type == Lexer.LexerTokenValue.DOT)
            {
                return ParseObjectCall(arr, ref index);
            }
            return ParseCall(arr, ref index);
        }

        private static ASTNode ParseObjectCall(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Object call must be a keyword {arr[index]}");

            var apiCall = new ASTNode(Lexer.LexerTokenValue.OBJECT_CALL, arr[index].RawValue);

            index++;
            while (arr[index].Type == Lexer.LexerTokenValue.DOT)
            {
                index++;
                apiCall.Children.Add(ParseBasicCall(arr, ref index));
            }

            return apiCall;
        }

        private static ASTNode ParseBasicCall(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Call must be a keyword {arr[index]}");

            var apiCall = new ASTNode(Lexer.LexerTokenValue.FUNCTION_CALL, arr[index].RawValue);
            index++;
            ParseParentheses(arr, ref index, ref apiCall);
            return apiCall;
        }

        private static ASTNode ParseCall(Lexer.LexerToken[] arr, ref int index)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.KEYWORD)
                throw new Exception($"Call must be a keyword {arr[index + 1]}");

            if (ExpectNext(arr, index, Lexer.LexerTokenValue.LPAREN))
            {
                return ParseBasicCall(arr, ref index);
            }
            else if (ExpectNext(arr, index, Lexer.LexerTokenValue.COLON))
            {
                var apiCall = new ASTNode(Lexer.LexerTokenValue.LIBRARY_CALL, arr[index].RawValue);
                index += 2;
                apiCall.Children.Add(ParseCall(arr, ref index));
                return apiCall;
            }
            throw new Exception($"Cannot parse call with object {arr[index + 1]}");
        }

        private static bool MethodHeader(Lexer.LexerToken[] arr, ref int index, out ASTNode node)
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

        private static int ParseParentheses(Lexer.LexerToken[] arr, ref int index, ref ASTNode node)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.LPAREN)
                throw new Exception($"Cannot read parentheses at {arr[index]}");

            int args = 0;
            bool comma = false;
            while (arr[++index].Type != Lexer.LexerTokenValue.RPAREN)
            {
                if (comma && arr[index].Type != Lexer.LexerTokenValue.COMMA)
                    throw new Exception($"Missing seperator at {arr[index]}");
                else if (arr[index].Type == Lexer.LexerTokenValue.RPAREN)
                    break;
                else if (!comma)
                {
                    args++;
                    node.Children.Add(ParseExpression(arr, ref index));
                    index--;
                }
                comma = !comma;
            }
            index++;
            return args;
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

        private static int ParseVariablesSimple(Lexer.LexerToken[] arr, ref int index, ref ASTNode node)
        {
            if (arr[index].Type != Lexer.LexerTokenValue.LPAREN)
                throw new Exception($"Cannot read parentheses at {arr[index]}");

            int args = 0;
            bool commaExpected = false;

            while (arr[++index].Type != Lexer.LexerTokenValue.RPAREN)
            {
                if (commaExpected)
                {
                    if (arr[index].Type != Lexer.LexerTokenValue.COMMA)
                        throw new Exception($"Missing separator at {arr[index]}");
                    index++;
                }

                if (arr[index].Type == Lexer.LexerTokenValue.RPAREN)
                    break;

                Lexer.LexerToken variableName = arr[index];
                if (arr[index + 1].Type != Lexer.LexerTokenValue.COLON)
                    throw new Exception($"Expected ':' after variable name at {arr[index + 1]}");

                Lexer.LexerToken variableType = arr[index + 2];
                if (!variableType.Type.IsVariable())
                    throw new Exception($"Invalid variable type at {variableType}");

                ASTNode variableNode = new ASTNode(Lexer.LexerTokenValue.VARIABLE, variableName.RawValue);
                variableNode.Children.Add(new ASTNode(variableType));
                node.Children.Add(variableNode);

                args++;
                index += 2;
                commaExpected = true;
            }

            index++;
            return args;
        }

    }
}
