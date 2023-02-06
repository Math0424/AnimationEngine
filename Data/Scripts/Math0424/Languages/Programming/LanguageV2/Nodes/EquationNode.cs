using AnimationEngine.Language;
using System.Collections.Generic;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class EquationNode : CompilationNode
    {
        Stack<Token> _output = new Stack<Token>();
        Stack<Token> _operators = new Stack<Token>();

        private int Precedence(TokenType token)
        {
            switch (token)
            {
                case TokenType.ADD:
                case TokenType.SUB:
                    return 1;
                case TokenType.MOD:
                case TokenType.DIV:
                case TokenType.MUL:
                    return 2;
                default: return 0;
            }

        }

        public EquationNode(ref int index, int stop)
        {
            bool before = Context.RequireReturn;
            Context.RequireReturn = true;
            while (index < stop && Tokens[index].Type != TokenType.COM && Tokens[index].Type != TokenType.ENDL)
            {
                Token t = Tokens[index];
                if (t.Type.IsMathVariable())
                {
                    _output.Push(t);
                }
                else if (t.Type == TokenType.KEWRD)
                {
                    if (Context.IsVariable(t.Value.ToString()) || Context.IsGlobalVariable(t.Value.ToString()))
                    {
                        _output.Push(t);
                        index++;
                        continue;
                    }

                    children.Add(new ClassCallNode(ref index));
                    _output.Push(new Token(TokenType.UKWN, children.Count - 1, t.Line, t.Col));
                }
                else if (t.Type.IsMathOperator())
                {
                    while (_operators.Count != 0)
                    {
                        if (_operators.Peek().Type == TokenType.LPAREN || Precedence(_operators.Peek().Type) < Precedence(t.Type))
                        {
                            break;
                        }
                        _output.Push(_operators.Pop());
                    }
                    _operators.Push(t);
                }
                else if (t.Type == TokenType.LPAREN)
                {
                    _operators.Push(t);
                }
                else if (t.Type == TokenType.RPAREN)
                {
                    while (true)
                    {
                        if (_operators.Count == 0)
                            throw Script.DetailedErrorLog("unbalanced parentheses!", t);
                        if (_operators.Peek().Type == TokenType.LPAREN)
                        {
                            _operators.Pop();
                            break;
                        }
                        _output.Push(_operators.Pop());
                    }
                }
                else
                {
                    throw Script.DetailedErrorLog($"Cannot parse token of type {t.Type}", t);
                }
                index++;
            }

            while (_operators.Count != 0)
            {
                if (_operators.Peek().Type == TokenType.LPAREN)
                {
                    throw Script.DetailedErrorLog("unbalanced parentheses!", _operators.Peek());
                }
                _output.Push(_operators.Pop());
            }
            Context.RequireReturn = before;
        }



        private void AddLineOperator(TokenType t, int o, int a, int b)
        {
            switch (t)
            {
                case TokenType.ADD:
                    Script.program.Add(new Line(ProgramFunc.Add, o, a, b));
                    break;
                case TokenType.SUB:
                    Script.program.Add(new Line(ProgramFunc.Sub, o, a, b));
                    break;
                case TokenType.MUL:
                    Script.program.Add(new Line(ProgramFunc.Mul, o, a, b));
                    break;
                case TokenType.DIV:
                    Script.program.Add(new Line(ProgramFunc.Div, o, a, b));
                    break;
                case TokenType.MOD:
                    Script.program.Add(new Line(ProgramFunc.Mod, o, a, b));
                    break;
            }
        }

        Stack<string> _stack = new Stack<string>();
        public override void Compile()
        {
            bool before = Context.RequireReturn;
            Context.RequireReturn = true;

            List<Token> tokens = new List<Token>(_output);
            tokens.Reverse();

            for (int i = 0; i < tokens.Count; i++)
            {
                Token t = tokens[i];
                switch (t.Type)
                {
                    case TokenType.ADD:
                    case TokenType.SUB:
                    case TokenType.MUL:
                    case TokenType.DIV:
                    case TokenType.MOD:
                        string b = _stack.Pop();
                        string a = _stack.Pop();
                        if (a.Length + b.Length == 0)
                        {
                            AddLineOperator(t.Type, 1, 1, 0);
                            Context.PopStackIndex();
                            Script.program.Add(new Line(ProgramFunc.PopJ, 1));
                            _stack.Push("");
                        }
                        else if (a.Length != 0 && b.Length != 0)
                        {
                            Context.IncreaseStackIndex();
                            Script.program.Add(new Line(ProgramFunc.LdI, Script.AddImmediate(new SVariableInt(0))));
                            AddLineOperator(t.Type, 0, Context.GetCompileVariableIndex(a), Context.GetCompileVariableIndex(b));
                            _stack.Push("");
                        }
                        else
                        {
                            AddLineOperator(t.Type, 0, (a.Length == 0 ? 0 : Context.GetCompileVariableIndex(a)), (b.Length == 0 ? 0 : Context.GetCompileVariableIndex(b)));
                            _stack.Push("");
                        }
                        break;

                    case TokenType.INT:
                    case TokenType.FLOAT:
                    case TokenType.BOOL:
                    case TokenType.MVECTOR:
                    case TokenType.STR:
                    case TokenType.LERP:
                        Context.IncreaseStackIndex();
                        Script.program.Add(new Line(ProgramFunc.LdI, Script.AddImmediate(SVarUtil.Convert(t))));
                        _stack.Push("");
                        break;
                    case TokenType.KEWRD: //variable
                        if (Context.IsCompileVariable(t.Value.ToString()))
                        {
                            if (tokens.Count == 1)
                            {
                                Context.IncreaseStackIndex();
                                Script.program.Add(new Line(ProgramFunc.LdI, Script.AddImmediate(new SVariableInt(0))));
                                Script.program.Add(new Line(ProgramFunc.Cpy, Context.GetCompileVariableIndex(t.Value.ToString()), 0));
                            }
                            else
                            {
                                _stack.Push(t.Value.ToString());
                            }
                        }
                        else if (Context.IsGlobalVariable(t.Value.ToString()))
                        {
                            Context.IncreaseStackIndex();
                            Script.program.Add(new Line(ProgramFunc.LdG, Context.GetGlobalVariable(t.Value.ToString())));
                            _stack.Push("");
                        }
                        else
                        {
                            throw Script.DetailedErrorLog($"Cannot find variable in scope", t);
                        }
                        break;

                    case TokenType.UKWN: //function call
                        children[(int)t.Value].Compile();
                        children[(int)t.Value].PostCompile();
                        _stack.Push("");
                        break;
                    default:
                        throw Script.DetailedErrorLog($"Canot read token of this type in equasion", t);
                }
            }

            Context.RequireReturn = before;
        }

        public override void PostCompile()
        {
            Context.PopStackIndex();
            Script.program.Add(new Line(ProgramFunc.PopJ, 1));
        }

    }
}
