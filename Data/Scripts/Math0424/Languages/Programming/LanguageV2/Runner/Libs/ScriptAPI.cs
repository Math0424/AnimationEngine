using System;

namespace AnimationEngine.CoreScript.Libs
{
    internal class ScriptAPI : ScriptLib
    {
        public ScriptAPI()
        {
            AddMethod("log", log);
        }


        public SVariable log(SVariable[] var)
        {
            Console.WriteLine("API Log: " + var[0].ToString());
            return null;
        }

    }
}
