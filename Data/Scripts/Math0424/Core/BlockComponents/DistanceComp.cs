using AnimationEngine.Core;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    internal class DistanceComp : BlockComponent
    {

        public Action InRange;
        public Action OutOfRange;

        private int tick;
        private float distance;
        private bool triggered;
        
        private List<IMyPlayer> characters = new List<IMyPlayer>();
        private IMyCubeBlock block;

        public DistanceComp(float distance)
        {
            this.distance = distance * distance;
        }

        public override void Initalize(IMyCubeBlock block)
        {
            this.block = block;
        }

        public override void Tick(int i)
        {
            if (block.CubeGrid.PlayerPresenceTier != MyUpdateTiersPlayerPresence.Normal)
            {
                return;
            }

            tick += i;
            if (tick % 60 == 0 || tick > 60)
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
                foreach(var p in characters)
                {
                    if (p.Character != null)
                    {
                        double dist = Vector3D.DistanceSquared(p.Character.GetPosition(), block.WorldMatrix.Translation);
                        if (dist < lowest)
                        {
                            lowest = dist;
                        }
                    }
                }

                if (lowest < distance && !triggered)
                {
                    InRange?.Invoke();
                    triggered = true;
                } 
                else if(lowest > distance && triggered)
                {
                    OutOfRange?.Invoke();
                    triggered = false;
                }

            }
        }
    }
}
