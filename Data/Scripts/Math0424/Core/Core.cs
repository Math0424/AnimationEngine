﻿using AnimationEngine.Language;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace AnimationEngine.Core
{
    internal struct Subpart
    {
        public Subpart(string name, string parent)
        {
            Name = name;
            Parent = parent;
        }
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
        public void Init(CoreScript parent);
        public void Tick(int time);
        public void Close();
    }

    internal interface ScriptRunner : EntityComponent
    {
        public ScriptRunner Clone();
        public void Execute(string function, params SVariable[] args);
    }

    internal interface Initializable
    {
        void Initalize(IMyEntity ent);
    }

    internal abstract class ScriptLib
    {
        protected Dictionary<string, Func<SVariable[], SVariable>> _dir = new Dictionary<string, Func<SVariable[], SVariable>>();

        public void AddMethod(string name, Func<SVariable[], SVariable?> func)
        {
            _dir[name] = func;
        }
        
        public void RemoveMethod(string name)
        {
            _dir.Remove(name);
        }

        public SVariable Execute(string value, SVariable[] arr)
        {
            if (_dir.ContainsKey(value))
            {
                return _dir[value].Invoke(arr);
            }
            return null;
        }
    }
}
