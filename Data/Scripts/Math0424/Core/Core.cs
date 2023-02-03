using AnimationEngine.Language;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace AnimationEngine.Core
{
    internal struct Subpart
    {
        public string Name;
        public string Parent;
    }

    public struct ObjectDef
    {
        public string Type;
        public string Name;
        public string Parent;
        public object[] Values;
        public ObjectDef(string Type, string Name, string Parent, params object[] Values)
        {
            this.Parent = Parent;
            this.Name = Name;
            this.Type = Type;
            this.Values = Values;
        }
    }

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
        public Dictionary<string, Action<SVariable[]>> Actions = new Dictionary<string, Action<SVariable[]>>();
        public virtual void Tick(int tick) { }
        public void Call(string action, params SVariable[] arr)
        {
            if (Actions.ContainsKey(action))
                Actions[action].Invoke(arr);
        }
    }
}
