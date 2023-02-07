using AnimationEngine.Core;
using AnimationEngine.Language;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    internal class DistanceComp : EntityComponent
    {

        public Action<SVariable> Changed;
        public Action InRange;
        public Action OutOfRange;

        private int tick;
        private float distance;
        private bool triggered;

        private List<IMyPlayer> characters = new List<IMyPlayer>();
        private IMyEntity entity;

        public DistanceComp(float distance)
        {
            this.distance = distance * distance;
        }

        public void Close()
        {

        }

        public void Init(CoreScript parent)
        {
            entity = parent.Entity;
        }

        public void Tick(int time)
        {
            tick += time;
            if (tick > 60)
            {
                tick = 0;
                characters.Clear();
                if (MyAPIGateway.Multiplayer != null)
                {
                    MyAPIGateway.Multiplayer.Players.GetPlayers(characters);
                }
                else
                {
                    characters.Add(MyAPIGateway.Session.Player);
                }

                double lowest = double.MaxValue;
                foreach (var p in characters)
                {
                    if (p.Character != null)
                    {
                        double dist = Vector3D.DistanceSquared(p.Character.GetPosition(), entity.WorldMatrix.Translation);
                        if (dist < lowest)
                            lowest = dist;
                    }
                }

                Changed?.Invoke(new SVariableFloat((float)lowest));
                if (lowest < distance && !triggered)
                {
                    InRange?.Invoke();
                    triggered = true;
                }
                else if (lowest > distance && triggered)
                {
                    OutOfRange?.Invoke();
                    triggered = false;
                }
            }
        }
    }
}
