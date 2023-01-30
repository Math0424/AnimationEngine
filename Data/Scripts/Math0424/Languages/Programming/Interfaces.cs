using System;
using System.Collections.Generic;
using System.Text;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Language
{
    internal abstract class ScriptGenerator
    {
        public ScriptError Error { get; protected set; }
        public string[] RawScript { get; protected set; }
        public List<Token> Tokens = new List<Token>();


        public ScriptGenerator(ModItem mod, string path)
        {

        }

    }
}
