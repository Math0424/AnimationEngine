﻿using AnimationEngine.Language;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;
using VRage.ModAPI;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Core
{
    internal struct Subpart
    {
        public Subpart(string customname, string name, string parent)
        {
            CustomName = customname;
            Name = name;
            Parent = parent;
        }
        public string CustomName;
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
        public abstract void Init(SubpartCore core);
        public abstract void Tick(int tick);
        public virtual void Close() { }
    }

    internal interface EntityComponent
    {
        void InitBuilt(CoreScript parent);
        void Tick(int time);
        void Close();
    }

    internal interface ScriptRunner : EntityComponent
    {
        ModItem GetMod();
        ScriptRunner Clone();
        void Stop();
        void Execute(string function, params SVariable[] args);
    }

    internal interface Parentable
    {
        string GetParent();
    }

    internal interface Initializable
    {
        void Init(IMyEntity ent);
    }

    internal abstract class ScriptLib
    {
        protected Dictionary<string, Func<SVariable[], SVariable>> _dir = new Dictionary<string, Func<SVariable[], SVariable>>();

        public void AddMethod(string name, Func<SVariable[], SVariable> func)
        {
            _dir[name] = func;
        }

        public void RemoveMethod(string name)
        {
            _dir.Remove(name);
        }

        public virtual SVariable Execute(string value, SVariable[] arr)
        {
            if (_dir.ContainsKey(value.ToLower()))
                return _dir[value.ToLower()].Invoke(arr);
            return null;
        }

        public virtual void Close() { }

        public virtual void Tick(int time) { }
    }
}
