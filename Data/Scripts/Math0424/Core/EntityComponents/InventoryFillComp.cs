using AnimationEngine.Core;
using AnimationEngine.Utility;
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

        IMyGasTank tank;
        private double PrevGasValue;

        private MyInventory inventory;
        private int lastUpdate, lastInvoked;

        public InventoryFillComp() { }

        public void Init(CoreScript parent)
        {
            if (parent.Entity is IMyGasTank)
            {
                tank = parent.Entity as IMyGasTank;
                PrevGasValue = tank.FilledRatio;
                Changed?.Invoke((float)PrevGasValue);
            }
            else if (parent.Entity.HasInventory)
            {
                inventory = (MyInventory)parent.Entity.GetInventory();
                inventory.ContentsChanged += Update;
                Changed?.Invoke(inventory.CurrentVolume.RawValue / (float)inventory.MaxVolume.RawValue);
            }
        }

        private void Update(MyInventoryBase inv)
        {
            if (lastInvoked == lastUpdate)
                return;

            lastInvoked = lastUpdate;
            Changed?.Invoke(inventory.CurrentVolume.RawValue / (float)inventory.MaxVolume.RawValue);
        }

        public void Close() { }
        public void Tick(int time) 
        {
            lastUpdate += time;
            if (tank != null && PrevGasValue != tank.FilledRatio)
            {
                PrevGasValue = tank.FilledRatio;
                Changed?.Invoke((float)PrevGasValue);
            }
        }
    }
}
