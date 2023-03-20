using AnimationEngine.Core;
using AnimationEngine.Utility;
using System.Collections.Generic;

namespace AnimationEngine.Language.Libs
{
    internal struct MethodTick
    {
        public MethodTick(string method, int loopDelay, int loopCount)
        {
            this.method = method;
            this.loopDelay = loopDelay;
            this.loopCount = loopCount;
            currDelay = 0;
        }
        public int loopDelay;
        public int loopCount;
        public int currDelay;
        public string method;
    }

    internal class ScriptAPI : ScriptLib
    {
        private List<MethodTick> methodLoops = new List<MethodTick>();
        private ScriptRunner runner;

        public ScriptAPI(ScriptRunner runner)
        {
            this.runner = runner;

            AddMethod("log", log);
            AddMethod("startloop", StartLoop);
            AddMethod("stoploop", StopLoop);
        }

        public SVariable log(SVariable[] var)
        {
            Utils.LogToFile("Log: " + var[0].ToString());
            return null;
        }

        public SVariable StartLoop(SVariable[] var)
        {
            methodLoops.Add(new MethodTick("func_" + var[0].ToString().ToLower(), var[1].AsInt(), var[2].AsInt()));
            return null;
        }

        public SVariable StopLoop(SVariable[] var)
        {
            foreach (var x in methodLoops)
            {
                if (x.method == ("func_" + var[0].ToString().ToLower()))
                {
                    methodLoops.Remove(x);
                    return null;
                }
            }
            return null;
        }

        public override void Tick(int time)
        {
            for (int i = 0; i < methodLoops.Count; i++)
            {
                MethodTick tick = methodLoops[i];
                tick.currDelay -= time;
                if (tick.currDelay <= 0)
                {
                    tick.currDelay = tick.loopDelay;
                    runner.Execute(tick.method);
                    if (tick.loopCount != -1)
                        tick.loopCount--;
                }
                methodLoops[i] = tick;
            }
            methodLoops.RemoveAll((e) => e.loopCount == 0);
        }

    }
}
