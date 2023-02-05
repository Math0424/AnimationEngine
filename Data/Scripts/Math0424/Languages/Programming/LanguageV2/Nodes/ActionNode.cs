using AnimationEngine.Language;
using System.Collections.Generic;
using System.Linq;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class ActionNode : CompilationNode
    {
        string name;
        int id;

        public ActionNode(ref int index, bool action)
        {
            if (Tokens[++index].Type != TokenType.KEWRD)
            {
                throw Script.DetailedErrorLog("Action must have a name type", Tokens[index]);
            }

            id = action ? Script.actions.Count : Script.terminals.Count;

            name = id + "_" + Tokens[index].Value.ToString().ToLower();
            ScriptAction act = new ScriptAction()
            {
                TokenName = Tokens[index].Value.ToString().ToLower(),
                Name = Tokens[index],
                ID = id,
            };
            index += 2;
            act.Paramaters = StrictVector(ref index, TokenType.RPAREN);
            act.Name.Value = name;

            index++;
            if (Tokens[index].Type != TokenType.VECTOR)
            {
                throw Script.DetailedErrorLog("Action needs a body", Tokens[index]);
            }

            List<Function> funcs = new List<Function>();
            Context.EnterNewContext((Token[])Tokens[index].Value);
            int next = Next(TokenType.KEWRD, 0);
            while (next != -1)
            {
                children.Add(new FunctionNode(ref next, $"{(action ? "act" : "term")}_{id}_"));
                funcs.Add(Script.functions.Values.Last());
                next = Next(TokenType.KEWRD, ++next);
            }
            Context.PopTopContext();
            act.Funcs = funcs.ToArray();

            if (act.Funcs.Length == 0)
            {
                throw Script.DetailedErrorLog($"Action must have children", act.Name);
            }

            string err;
            if (!LanguageDictionary.IsAction(act, action, out err))
            {
                throw Script.DetailedErrorLog($"Action has error '{err}'", act.Name);
            }

            if (action)
                Script.actions.Add(act);
            else
                Script.terminals.Add(act);

        }

        public override void Compile()
        {
            foreach (var x in children)
            {
                x.Compile();
                x.PostCompile();
            }
        }

        public override void PostCompile()
        {
        }
    }
}
