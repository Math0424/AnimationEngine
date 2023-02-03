using AnimationEngine.Core;
using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.LanguageV2
{
    internal class ScriptV2Runner : ScriptRunner
    {
        CoreScript core;

        public void Execute(string entity, string method, params SVariable[] args)
        {

        }

        public void Init(CoreScript script)
        {
            core = script;
        }

        public void Tick(int time)
        {

        }

        public void Close()
        {

        }
    }
}
