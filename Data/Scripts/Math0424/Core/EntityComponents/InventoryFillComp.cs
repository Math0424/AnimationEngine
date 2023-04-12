using AnimationEngine.Core;
using Sandbox.Game;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Entity;

namespace AnimationEngine
{
    internal class InventoryFillComp : EntityComponent
    {
        public Action<float> Changed;

        private MyInventory inventory;
        private int lastUpdate, lastInvoked;

        public InventoryFillComp() { }

        public void Init(CoreScript parent)
        {
            if (parent.Entity.HasInventory)
            {
                inventory = (MyInventory)parent.Entity.GetInventory();
                inventory.ContentsChanged += Update;
            }
        }

        private void Update(MyInventoryBase inv)
        {
            if (lastInvoked == lastUpdate)
                return;

            lastInvoked = lastUpdate;
            float curr = (inventory.CurrentVolume.ToIntSafe() / inventory.MaxVolume.ToIntSafe());
            Changed?.Invoke(curr);
        }

        public void Close() { }
        public void Tick(int time) 
        {
            lastUpdate += time;
        }
    }
}
