using AnimationEngine.Core;
using AnimationEngine.Utility;

namespace AnimationEngine.Language.Libs
{
    internal class ScriptAPI : ScriptLib
    {
        public ScriptAPI()
        {
            AddMethod("log", log);
        }

        public SVariable log(SVariable[] var)
        {
            Utils.LogToFile("API Log: " + var[0].ToString());
            return null;
        }

    }
}
