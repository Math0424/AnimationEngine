using AnimationEngine.Utility;
using Sandbox.Game.Components;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace AnimationEngine.Core
{
    internal class CoreScript
    {
        public IMyEntity Entity { private set; get; }
        public long EntityId { private set; get; }
        public long ParentId { private set; get; }
        private List<EntityComponent> components = new List<EntityComponent>();
        public Dictionary<string, SubpartCore> Subparts = new Dictionary<string, SubpartCore>();
        private Dictionary<string, Subpart> subpartData = new Dictionary<string, Subpart>();

        //1 = just created
        //2 = just ready
        public int Flags = 0;

        string modName;
        public string GetModName()
        {
            return modName;
        }

        public CoreScript(string name, Subpart[] subparts)
        {
            modName = name;
            foreach (var x in subparts)
                subpartData[x.CustomName] = x;
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
            EntityId = ent.EntityId;
            ParentId = ent.Parent.EntityId;
            foreach (var subpart in subpartData.Values)
            {
                Subparts[subpart.CustomName] = new SubpartCore();
                if (!InitSubpart(subpart))
                    unReadySubparts.Add(subpart.CustomName);
            }

            for (int i = 0; i < components.Count; i++)
                components[i].Init(this);

            ent.OnClosing += OnClose;
            AnimationEngine.AddScript(this);
            Flags |= 1;
        }

        private bool InitSubpart(Subpart subpart)
        {
            if (!Subparts.ContainsKey(subpart.CustomName))
                return false;

            if (Subparts[subpart.CustomName].Subpart != null && !Subparts[subpart.CustomName].Subpart.MarkedForClose && !Subparts[subpart.CustomName].Subpart.Closed)
                return true;

            MyEntitySubpart part;
            if (subpart.Parent != null)
            {
                if (Subparts[subpart.Parent].Subpart == null)
                    return false;
                if (!Subparts[subpart.Parent].Subpart.TryGetSubpart(subpart.Name, out part))
                {
                    return false;
                }
            } 
            else if(!Entity.TryGetSubpart(subpart.Name, out part))
            {
                return false;
            }

            if (part.Render.GetType() != typeof(MyRenderComponent))
            {
                if (part.Model == null)
                    return false;
                
                string asset = ((IMyModel)part.Model).AssetName;
                var model = part.Render.ModelStorage;
                var matrix = part.PositionComp.LocalMatrixRef;
                var physics = part.Physics;

                part.OnClose -= SubpartClose;
                part.Close();

                part = new MyEntitySubpart();
                part.Render.EnableColorMaskHsv = Entity.Render.EnableColorMaskHsv;
                part.Render.ColorMaskHsv = Entity.Render.ColorMaskHsv;
                part.Render.TextureChanges = Entity.Render.TextureChanges;
                part.Render.MetalnessColorable = Entity.Render.MetalnessColorable;
                part.Physics = physics;

                part.Init(null, asset, (MyEntity)Entity, null, null);
                part.OnAddedToScene(Entity);
                part.PositionComp.SetLocalMatrix(ref matrix, null, true);
                ((MyEntity)Entity).Subparts[subpart.Name] = part;
            }

            part.Name = subpart.CustomName;
            part.OnClose += SubpartClose;
            Subparts[subpart.CustomName].Init(part);
            return true;
        }

        private void SubpartClose(IMyEntity ent)
        {
            if (ent != null && Subparts.ContainsKey(ent.Name))
            {
                Subparts[ent.Name].Close();
                Subparts[ent.Name].Subpart.OnClose -= SubpartClose;
                unReadySubparts.Add(ent.Name);
            }
        }

        List<string> unReadySubparts = new List<string>();
        public void Tick(int time)
        {
            if (unReadySubparts.Count != 0)
            {
                Flags |= 2;
                List<string> ready = new List<string>();
                foreach (var x in unReadySubparts)
                {
                    if (InitSubpart(subpartData[x]))
                        ready.Add(x);
                }
                unReadySubparts.RemoveAll((e) => ready.Contains(e));
                if (unReadySubparts.Count == 0)
                {
                    for (int i = 0; i < components.Count; i++)
                        components[i].Init(this);
                }
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
