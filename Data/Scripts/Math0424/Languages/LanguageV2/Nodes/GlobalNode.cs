
using AnimationEngine.CoreScript;
using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;
using AnimationEngine.CoreScript;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class GlobalNode : CompilationNode
    {
        Token t;
        string name;
        public GlobalNode(int start)
        {
            if (start + 5 > Tokens.Length || Tokens[start + 2].Type != TokenType.EQL || Tokens[start + 3].Type == TokenType.KEWRD)
            {
                throw Script.DetailedErrorLog($"Invalid global definition (var [name] = [value])", Tokens[start + 1]);
            }

            name = Tokens[start + 1].Value.ToString().ToLower();
            if (Context.IsVariable(name) || Context.IsGlobalVariable(name))
            {
                throw Script.DetailedErrorLog($"Duplicate global", Tokens[start + 1]);
            }

            t = Tokens[start + 3];
            if (!(t.Type == TokenType.MVECTOR || t.Type == TokenType.INT || t.Type == TokenType.FLOAT || t.Type == TokenType.BOOL))
            {
                throw Script.DetailedErrorLog($"Invalid global variable type, must be type INT, MVECTOR or FLOAT", t);
            }

            if (Script.objects.Exists((e) => e.Name.Value.ToString().ToLower().Equals(name)))
            {
                throw Script.DetailedErrorLog($"Global cannot have the same name as an object", Tokens[start + 1]);
            }

            Context.AddGlobalVariable(name);
            Script.globalCount++;
        }
        
        public override void Compile()
        {
            Script.globals.Add(SVarUtil.Convert(t));
        }

        public override void PostCompile() { }

    }
}
