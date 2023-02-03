using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.Core
{
    internal interface ScriptRunner : EntityComponent
    {

        public ScriptRunner Clone();

        public void Execute(string entity, string method, params SVariable[] args);

    }
}
