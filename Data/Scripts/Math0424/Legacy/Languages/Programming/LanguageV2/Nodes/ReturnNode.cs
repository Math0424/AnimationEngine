﻿using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class ReturnNode : CompilationNode
    {

        public ReturnNode(ref int index)
        {
            index++;
            if (Tokens[index].Type != TokenType.ENDL)
            {
                children.Add(new EquationNode(ref index, Next(TokenType.ENDL, index)));
            }
        }

        public override void Compile()
        {
            if (children.Count != 0)
            {
                children[0].Compile();
                Context.PopStackIndex();
                Script.program.Add(new Line(ProgramFunc.Cpy, 0, Context.GetStackSize()));
                Script.program.Add(new Line(ProgramFunc.Pop, Context.GetStackSize()));
                Script.program.Add(new Line(ProgramFunc.End));
            }
            else
            {
                if (Context.GetStackSize() != 0)
                    Script.program.Add(new Line(ProgramFunc.Pop, Context.GetStackSize()));
                Script.program.Add(new Line(ProgramFunc.LdI, 0));
                Script.program.Add(new Line(ProgramFunc.End));
            }
        }

        public override void PostCompile() { }
    }
}
