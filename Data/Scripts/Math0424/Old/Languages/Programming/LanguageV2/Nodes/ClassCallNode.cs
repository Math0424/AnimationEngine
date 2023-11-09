using AnimationEngine.Language;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class ClassCallNode : CompilationNode
    {
        Token t;
        string objectName;
        bool function = false;
        string prevousContext;

        public ClassCallNode(ref int index)
        {
            t = Tokens[index];
            if (t.Type != TokenType.KEWRD)
            {
                throw Script.DetailedErrorLog("Not a function call", t);
            }

            objectName = t.Value.ToString().ToLower();
            if (Tokens[index + 1].Type == TokenType.LPAREN)
            {
                function = true;
                children.Add(new FunctionCallNode(ref index));
            }
            else if (Tokens[index + 1].Type == TokenType.DOT)
            {
                prevousContext = Context.ClassContext;
                Context.ClassContext = "";
                foreach (var x in Script.objects)
                {
                    if (x.Name.Value.ToString().ToLower() == objectName)
                    {
                        Context.ClassContext = x.Type.Value.ToString();
                        break;
                    }
                }
                if (Context.ClassContext.Length == 0)
                    throw Script.DetailedErrorLog("Cannot find object", t);
                index += 2;
                children.Add(new MethodCallNode(ref index));
                Context.ClassContext = prevousContext;
            }
            else
            {
                throw Script.DetailedErrorLog("Not a local variable or method call.", t);
            }
        }

        public override void Compile()
        {
            if (function)
            {
                children[0].Compile();
            }
            else
            {
                int contextId = 0;
                for (; contextId <= Script.objects.Count; contextId++)
                {
                    if (contextId == Script.objects.Count)
                    {
                        throw Script.DetailedErrorLog("Cannot find object context", t);
                    }
                    if (Script.objects[contextId].Name.Value.ToString().ToLower().Equals(objectName))
                        break;
                }
                Script.program.Add(new Line(ProgramFunc.RDly));
                ((MethodCallNode)children[0]).ContextCompile(contextId);
            }
        }

        public override void PostCompile()
        {
            children[0].PostCompile();
        }
    }
}
