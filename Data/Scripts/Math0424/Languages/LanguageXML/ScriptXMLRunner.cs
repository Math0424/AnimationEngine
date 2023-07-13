using AnimationEngine.Core;
using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.LanguageXML
{
    internal class ScriptXMLRunner : ScriptRunner
    {
        string modName;
        List<sXMLDelay> delays = new List<sXMLDelay>();

        public ScriptRunner Clone()
        {
            return new ScriptXMLRunner(modName);
        }

        public string GetModName()
        {
            return modName;
        }

        public ScriptXMLRunner(string modName)
        {
            this.modName = modName;

        }

        public void Execute(string function, params SVariable[] args)
        {

        }

        public void InitBuilt(CoreScript parent)
        {

        }

        public void Tick(int time)
        {
            delays.ForEach(d => d.delay -= time);
            foreach(var d in delays)
                if (d.delay < 0)
                    foreach (var m in d.movements)
                        Execute(m.type, m.args);
            delays.RemoveAll(d => d.delay < 0);
        }

        public void Stop()
        {
            delays.Clear();
        }

        public void Close()
        {
            delays.Clear();
        }


    }

    public struct sTrigger
    {
        public string name;
        public SVariable[] args;
    }

    public struct sSubpartFrame
    {
        public string name;
        public int timeTaken;
        public sXMLDelay[] delays;
    }

    public struct sXMLDelay
    {
        public int delay;
        public sXMLMovement[] movements;
        public sXMLFunction[] functions;
    }

    public struct sXMLFunction
    {
        public string type;
        public SVariable[] args;
    }

    public struct sXMLMovement
    {
        public string type;
        public SVariable[] args;
    }
}
