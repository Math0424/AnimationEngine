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
        private Subpart[] subpartDefs;
        private Dictionary<Type, EntityComponent> components = new Dictionary<Type, EntityComponent>();
        public Dictionary<string, SubpartCore> Subparts = new Dictionary<string, SubpartCore>();

        public CoreScript(Subpart[] subparts)
        {
            this.subpartDefs = subparts;
        }

        public bool HasComponent<T>() where T : EntityComponent
        {
            return components.ContainsKey(typeof(T));
        }

        public T GetComponent<T>() where T : EntityComponent
        {
            return (T)components[typeof(T)];
        }

        public void AddComponent<T>(T comp) where T : EntityComponent
        {
            components[typeof(T)] = comp;
        }

        public void Init(IMyEntity ent)
        {
            Entity = ent;
            InitSubparts();
            foreach (var component in components.Values)
                component.Init(this);
            ent.OnClosing += OnClose;
            AnimationEngine.AddScript(this);
        }

        private void InitSubparts()
        {
            if (Entity.MarkedForClose)
                return;

            foreach (var subpart in subpartDefs)
                if (Subparts[subpart.Name] != null && Subparts[subpart.Name].Subpart != null)
                    Subparts[subpart.Name].Subpart.OnClose -= SubpartClose;
            
            foreach (var subpart in subpartDefs)
            {
                Subparts[subpart.Name]?.Close(null);

                MyEntitySubpart part; 
                if ((!Entity.TryGetSubpart(subpart.Name, out part)) || (subpart.Parent != null && !Subparts[subpart.Parent].Subpart.TryGetSubpart(subpart.Name, out part)))
                {
                    Utils.LogToFile($"Cannot find subpart {subpart.Name}:{subpart.Parent}");
                    continue;
                }
                part.Name = subpart.Name;
                part.OnClose += SubpartClose;
                Subparts[subpart.Name] = new SubpartCore(part);
            }
        }

        private void SubpartClose(IMyEntity ent)
        {
            InitSubparts();
        }

        public void Tick(int time)
        {
            foreach (var component in components.Values)
                component.Tick(time);
        }

        private void OnClose(IMyEntity ent)
        {
            AnimationEngine.RemoveScript(this);
            
            foreach (var component in components.Values)
                component.Close();
            Entity = null;
        }

    }
}
