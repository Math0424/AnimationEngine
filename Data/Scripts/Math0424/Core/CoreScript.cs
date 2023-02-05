using AnimationEngine.Language;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace AnimationEngine.Core
{
    internal class CoreScript
    {
        public IMyEntity Entity { private set; get; }
        private List<EntityComponent> components = new List<EntityComponent>();
        public Dictionary<string, SubpartCore> Subparts = new Dictionary<string, SubpartCore>();
        private Dictionary<string, Subpart> subpartData = new Dictionary<string, Subpart>();

        public CoreScript(Subpart[] subparts)
        {
            foreach (var x in subparts)
                subpartData[x.Name] = x;
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

        public void Init(IMyEntity ent)
        {
            Entity = ent;
            foreach (var subpart in subpartData.Values)
            {
                Subparts[subpart.Name] = new SubpartCore();
                if (!InitSubpart(subpart))
                    unReadySubparts.Add(subpart.Name);
            }

            for (int i = 0; i < components.Count; i++)
                components[i].Init(this);

            ent.OnClosing += OnClose;
            AnimationEngine.AddScript(this);
        }

        private bool InitSubpart(Subpart subpart)
        {
            if (Subparts[subpart.Name].Subpart != null && !Subparts[subpart.Name].Subpart.MarkedForClose)
                return true;

            MyEntitySubpart part;
            if (!Entity.TryGetSubpart(subpart.Name, out part) || (subpart.Parent != null && !Subparts[subpart.Parent].Subpart.TryGetSubpart(subpart.Name, out part)))
            {
                //Utils.LogToFile($"Cannot find subpart '{subpart.Name}:{subpart.Parent}'");
                return false;
            }
            part.Name = subpart.Name;
            part.OnClose += SubpartClose;
            Subparts[subpart.Name].Init(part);
            return true;
        }

        private void SubpartClose(IMyEntity ent)
        {
            Subparts[ent.Name].Close();
            Subparts[ent.Name].Subpart.OnClose -= SubpartClose;
            unReadySubparts.Add(ent.Name);
        }

        List<string> unReadySubparts = new List<string>();
        public void Tick(int time)
        {
            if (unReadySubparts.Count != 0)
            {
                List<string> ready = new List<string>();
                foreach(var x in unReadySubparts)
                    if (InitSubpart(subpartData[x]))
                        ready.Add(x);
                unReadySubparts.RemoveAll((e) => ready.Contains(e));
                return;
            }

            foreach (var component in components)
                component.Tick(time);
            foreach (var x in Subparts.Values)
                x.Tick(time);
        }

        private void OnClose(IMyEntity ent)
        {
            AnimationEngine.RemoveScript(this);
            
            foreach (var component in components)
                component.Close();
            Entity = null;
        }

    }
}
