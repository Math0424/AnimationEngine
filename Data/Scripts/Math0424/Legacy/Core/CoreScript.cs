﻿using AnimationEngine.Utility;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.ModAPI;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Core
{
    internal class CoreScript
    {
        public ModItem Mod { private set; get; }
        public IMyEntity Entity { private set; get; }
        public long EntityId { private set; get; }
        public long ParentId { private set; get; }
        private List<EntityComponent> components = new List<EntityComponent>();

        // I hate this
        private List<string> FlattenedSubparts = new List<string>();
        private Subpart[] subpartData;

        public Dictionary<string, List<SubpartCore>> SubpartArr = new Dictionary<string, List<SubpartCore>>();
        public Dictionary<string, SubpartCore> Subparts = new Dictionary<string, SubpartCore>();

        public enum BlockFlags
        {
            Created = 1,
            Built = 2,
            SubpartReady = 4,
            ActionsInited = 8,
            WeaponcoreInit = 16,
            ToolcoreInit = 32,
        }
        public BlockFlags Flags = 0;

        public CoreScript(ModItem mod, Subpart[] subparts)
        {
            this.Mod = mod;
            subpartData = subparts;
        }

        public void FlattenSubparts(List<string> subparts)
        {
            FlattenedSubparts = subparts;
        }

        public void Init(IMyEntity ent)
        {
            Entity = ent;
            EntityId = ent.EntityId;
            ParentId = ent.Parent.EntityId;

            Flags |= BlockFlags.Created;
            CreateSubparts(ent);
            ((MyEntity)ent).OnModelRefresh += CreateSubparts;

            ent.OnClosing += OnClose;
            AnimationEngine.AddScript(this);
        }

        private void PrepareFalttenedSubpart(MyEntity part)
        {
            if (FlattenedSubparts.Count == 0 || part == null || part.Subparts.Count == 0)
                return;

            foreach(var x in SubpartArr.Values)
                foreach(var y in x)
                    y?.Close();
            SubpartArr.Clear();

            foreach (var x in part.Subparts)
            {
                if (FlattenedSubparts.Contains(x.Key))
                {
                    if (!SubpartArr.ContainsKey(x.Key))
                        SubpartArr.Add(x.Key, new List<SubpartCore>());

                    SubpartCore core = new SubpartCore(x.Value, x.Key);
                    SubpartArr[x.Key].Add(core);
                }
                PrepareFalttenedSubpart(x.Value);
            }
        }

        private bool InitSubpart(Subpart subpart, ref SubpartCore core)
        {
            if (core.Valid)
                return true;

            if (!Subparts.ContainsKey(subpart.CustomName))
                Subparts[subpart.CustomName] = core;

            if (Subparts[subpart.CustomName].Subpart != null && !Subparts[subpart.CustomName].Subpart.MarkedForClose && !Subparts[subpart.CustomName].Subpart.Closed)
                return false;

            MyEntitySubpart part;
            if (subpart.Parent != null)
            {
                if (Subparts[subpart.Parent].Subpart == null)
                    return false;
                if (!Subparts[subpart.Parent].Subpart.TryGetSubpart(subpart.Name, out part))
                    return false;
            }
            else if(!Entity.TryGetSubpart(subpart.Name, out part))
            {
                return false;
            }

            core.SetSubpart(part);
            return true;
        }

        private void CreateSubparts(IMyEntity e)
        {
            foreach (var x in components)
                if (x is ScriptRunner)
                    ((ScriptRunner)x).Stop();

            Flags &= ~BlockFlags.Built;
            Flags |= BlockFlags.SubpartReady;

            foreach (var x in subpartData)
            {
                SubpartCore core;
                if (Subparts.ContainsKey(x.CustomName))
                    core = Subparts[x.CustomName];
                else
                    core = new SubpartCore(null, x.CustomName);

                if (!InitSubpart(x, ref core))
                {
                    Flags &= ~BlockFlags.SubpartReady;
                    break;
                }
            }

            if (Flags.HasFlag(BlockFlags.SubpartReady))
            {
                PrepareFalttenedSubpart(((MyEntity)e));

                Flags |= BlockFlags.Built;
                for (int i = 0; i < components.Count; i++)
                    components[i].InitBuilt(this);
            }
        }

        public void Tick(int time)
        {
            if (Entity == null)
                return;

            if (!Flags.HasFlag(BlockFlags.SubpartReady))
            {
                CreateSubparts(Entity);
                return;
            }

            foreach (var component in components)
                component.Tick(time);

            foreach (var x in Subparts.Values)
                if (x.Valid)
                    x.Tick(time);
                else
                    Flags &= ~BlockFlags.SubpartReady;

            //goofy aaah
            foreach(var x in SubpartArr.Values)
                foreach(var y in x)
                    if (y.Valid)
                        y.Tick(time);
        }

        public void OnClose(IMyEntity ent)
        {
            try
            {
                ((MyEntity)ent).OnModelRefresh -= CreateSubparts;
                AnimationEngine.RemoveScript(this);
                foreach (var x in components)
                    x.Close();
                foreach (var x in Subparts.Values)
                    x.Close();
            }
            catch (Exception ignored)
            {

            }
            Entity = null;
        }
        
        public void AddComponent<T>(T comp) where T : EntityComponent
        {
            components.Add(comp);
        }

        public T GetFirstComponent<T>() where T : EntityComponent
        {
            foreach (var x in components)
                if (x.GetType() == typeof(T))
                    return (T)x;
            return default(T);
        }

        public bool HasComponent<T>() where T : EntityComponent
        {
            foreach (var x in components)
                if (x.GetType() == typeof(T))
                    return true;
            return false;
        }

    }
}
