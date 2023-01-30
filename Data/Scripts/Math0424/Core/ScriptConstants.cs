using AnimationEngine.Language;
using AnimationEngine.LanguageV1;
using AnimationEngine.LogicV1;
using System.Collections.Generic;

namespace AnimationEngine.Core
{
    internal class ScriptConstants
    {
        public struct ObjectDef
        {
            public TokenType Type;
            public string Name;
            public object[] Values;
            public ObjectDef(TokenType Type, string Name, params object[] Values)
            {
                this.Name = Name;
                this.Type = Type;
                this.Values = Values;
            }
        }

        public Dictionary<string, Caller[]> Calls = new Dictionary<string, Caller[]>();
        public List<V1ScriptAction> ScriptActions = new List<V1ScriptAction>();
        public List<ObjectDef> ObjectDefs = new List<ObjectDef>();
    }
}
