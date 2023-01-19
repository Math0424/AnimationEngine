using AnimationEngine.Language;
using AnimationEngine.Logic;
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
        public List<ScriptAction> ScriptActions = new List<ScriptAction>();
        public List<ObjectDef> ObjectDefs = new List<ObjectDef>();
    }
}
