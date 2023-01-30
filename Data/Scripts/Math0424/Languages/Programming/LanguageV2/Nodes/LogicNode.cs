using AnimationEngine.CoreScript;
using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;
using AnimationEngine.CoreScript;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class LogicNode : CompilationNode
    {
        TokenType logicOperator = TokenType.UKWN;
        TokenType logicType = TokenType.UKWN;
        bool isNot = false;

        private bool IsEquasion(int index, out int b)
        {
            int balance = 0;
            while (index < Tokens.Length && !(Tokens[index].Type == TokenType.ENDL || Tokens[index].Type == TokenType.NOT || Tokens[index].Type.IsLogicOperator() || Tokens[index].Type.IsLogic()))
            {
                Token t = Tokens[index];
                if (t.Type == TokenType.LPAREN)
                {
                    balance++;
                }
                else if (t.Type == TokenType.RPAREN)
                {
                    balance--;
                    if (balance <= -1)
                    {
                        b = index;
                        return true;
                    }
                }
                else if(t.Type != TokenType.KEWRD && !t.Type.IsMathOperator() && !t.Type.IsMathVariable())
                {
                    throw Script.DetailedErrorLog($"Cannot parse logic token of type {t.Type}", t);
                }
                index++;
            }
            b = index;
            return balance == 0;
        }

        private bool CreateLogOrEquasion(ref int index)
        {
            int end;
            if (IsEquasion(index, out end))
            {
                children.Add(new EquationNode(ref index, end));
                return true;
            }
            else
            {
                index++;
                children.Add(new LogicNode(ref index));
                return false;
            }
        }

        public LogicNode(ref int index)
        {
            if (Tokens[index].Type == TokenType.NOT)
            {
                isNot = true;
                index++;
            }

            if (CreateLogOrEquasion(ref index))
            {
                if (!Tokens[index].Type.IsLogic())
                {
                    throw Script.DetailedErrorLog("Cannot find logical symbol", Tokens[index]);
                }
                logicType = Tokens[index].Type;
                
                index++;
                CreateLogOrEquasion(ref index);
            } 
            else
            {
                index++;
            }

            if (Tokens[index].Type.IsLogicOperator())
            {
                logicOperator = Tokens[index].Type;
                index++;
                children.Add(new LogicNode(ref index));
            }
        }

        protected int FindClose(int start)
        {
            int balance = 0;
            while (start < Tokens.Length)
            {
                if (Tokens[start].Type == TokenType.LPAREN)
                    balance--;
                if (Tokens[start].Type == TokenType.RPAREN)
                    balance++;
                if ((balance == 0 && Tokens[start].Type.IsLogic() || Tokens[start].Type.IsLogicOperator()) || balance == 1)
                    return start;
                start++;
            }
            return -1;
        }

        public override void Compile()
        {
            if (logicType != TokenType.UKWN) // only applies for [equasion logic equasion]
            {
                switch (logicType)
                {
                    case TokenType.GRT:    CompileWithBranch(ProgramFunc.BNEZ, 0, 1);    break;
                    case TokenType.LST:    CompileWithBranch(ProgramFunc.BNEZ, 1, 0);    break;
                    case TokenType.GRTE:   CompileWithBranch(ProgramFunc.BNE, 0, 1);   break;
                    case TokenType.LSTE:   CompileWithBranch(ProgramFunc.BNE, 1, 0);   break;
                    case TokenType.NOTEQ:  CompileWithBranch(ProgramFunc.BZ, 0, 1);     break;
                    case TokenType.COMP:   CompileWithBranch(ProgramFunc.BNZ, 0, 1);    break;
                }
            }

            if (logicOperator != TokenType.UKWN)
            {
                if (children.Count == 2)
                    children[0].Compile();

                //compare result of left
                Script.program.Add(new Line(ProgramFunc.Cmp, 0));

                //prepare output result
                Script.program.Add(new Line(ProgramFunc.PopJ, 1));
                Script.program.Add(new Line(ProgramFunc.LdI, Script.AddImmediate(new SVariableInt(1))));

                // 0 == true
                // 1 == false

                int index;
                int jumpTo;
                switch (logicOperator)
                {
                    case TokenType.AND:
                        index = Script.program.Count;
                        Script.program.Add(new Line(ProgramFunc.BNZ)); // jump to end if false

                        children[children.Count - 1].Compile(); // compile output
                        Script.program.Add(new Line(ProgramFunc.Cmp, 0));
                        Script.program.Add(new Line(ProgramFunc.PopJ, 1));

                        jumpTo = Script.program.Count + 2;
                        Script.program.Add(new Line(ProgramFunc.BNZ, jumpTo));
                        Script.program[index] = new Line(ProgramFunc.BNZ, jumpTo); // jump past subtract
                        Script.program.Add(new Line(ProgramFunc.Sub, 0, 0, 0));
                        break;
                    case TokenType.OR:
                        index = Script.program.Count;
                        Script.program.Add(new Line(ProgramFunc.BZ)); // jump to end if true

                        children[children.Count - 1].Compile(); // compile output
                        Script.program.Add(new Line(ProgramFunc.Cmp, 0));
                        Script.program.Add(new Line(ProgramFunc.PopJ, 1));

                        jumpTo = Script.program.Count + 1;
                        Script.program.Add(new Line(ProgramFunc.BNZ, jumpTo + 1));
                        Script.program[index] = new Line(ProgramFunc.BZ, jumpTo); // jump past subtract
                        Script.program.Add(new Line(ProgramFunc.Sub, 0, 0, 0));
                        break;
                }
                if (isNot)
                    Script.program.Add(new Line(ProgramFunc.bXor, 0, 0, 1));
            }

            if (logicOperator == TokenType.UKWN && logicType == TokenType.UKWN)
            {
                children[0].Compile();
                if (isNot)
                    Script.program.Add(new Line(ProgramFunc.bXor, 0, 0, 1));
            }
        }

        private void CompileWithBranch(ProgramFunc func, int a, int b)
        {
            Context.IncreaseStackIndex();
            Script.program.Add(new Line(ProgramFunc.LdI, Script.AddImmediate(new SVariableInt(1))));

            children[0].Compile();
            children[1].Compile();

            Script.program.Add(new Line(ProgramFunc.Sub, 0, a, b));
            Script.program.Add(new Line(ProgramFunc.Cmp, 0));

            children[0].PostCompile();
            children[1].PostCompile();

            Script.program.Add(new Line(func, Script.program.Count + 2));
            Script.program.Add(new Line(ProgramFunc.Sub, 0, 0, 0));
        }

        public override void PostCompile() {}

    }
}
