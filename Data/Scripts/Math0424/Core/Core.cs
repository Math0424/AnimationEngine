using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace AnimationEngine.Core
{
    internal abstract class SubpartComponent
    {
        public abstract void Initalize(SubpartCore core);
        public abstract void Tick(int tick);
        public virtual void Close() { }
    }

    internal abstract class BlockComponent
    {
        public abstract void Initalize(IMyCubeBlock block);
        public abstract void Tick(int tick);
    }

    internal interface Initializable
    {
        string GetParent();
        void Initalize(IMyEntity ent);
    }

    internal abstract class Actionable
    {
        public Dictionary<string, Action<object[]>> Actions = new Dictionary<string, Action<object[]>>();
        public virtual void Tick(int tick) { }
        public void Call(string action, params object[] arr)
        {
            if (!Actions.ContainsKey(action))
                return;
            Actions[action].Invoke(arr);
        }
    }
}
