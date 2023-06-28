using AnimationEngine.Core;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Input;
using VRageMath;

namespace AnimationEngine.Language.Libs
{
    internal struct MethodTick
    {
        public MethodTick(string method, int loopDelay, int loopCount, int startDelay)
        {
            this.method = method;
            this.loopDelay = loopDelay;
            this.loopCount = loopCount;
            this.currDelay = startDelay;
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
            AddMethod("startloop", startLoop);
            AddMethod("stoploop", stopLoop);

            AddMethod("stopdelays", stopDelays);
            AddMethod("assert", assert);

            AddMethod("getinputposition", getPositionDelta);
            AddMethod("getinputrotation", getRotation);
        }

        public SVariable getPositionDelta(SVariable[] var)
        {
            return new SVariableVector(MyAPIGateway.Input.GetPositionDelta());
        }

        public SVariable getRotation(SVariable[] var)
        {
            var vec = MyAPIGateway.Input.GetRotation();
            return new SVariableVector(new Vector3(vec.X, vec.Y, MyAPIGateway.Input.GetRoll()));
        }
        
        public SVariable stopDelays(SVariable[] var)
        {
            runner.Stop();
            return null;
        }

        public SVariable assert(SVariable[] var)
        {
            if (!var[0].Equals(var[1]))
            {
                throw new System.Exception($"assertion failed, expected {var[0].GetType()}({var[0].AsFloat()}) got {var[1].GetType()}({var[1].AsFloat()})");
            } 
            else
            {
                Utils.LogToFile($"Assert: {var[0].GetType()}({var[0].AsFloat()}) == {var[1].GetType()}({var[1].AsFloat()})");
            }
            return null;
        }

        public SVariable log(SVariable[] var)
        {
            Utils.LogToFile("Log: " + var[0].ToString());
            return null;
        }

        public SVariable startLoop(SVariable[] var)
        {
            if (var.Length == 3)
                methodLoops.Add(new MethodTick("func_" + var[0].ToString().ToLower(), var[1].AsInt(), var[2].AsInt(), 0));
            else
                methodLoops.Add(new MethodTick("func_" + var[0].ToString().ToLower(), var[1].AsInt(), var[2].AsInt(), var[3].AsInt()));
            return null;
        }

        public SVariable stopLoop(SVariable[] var)
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
