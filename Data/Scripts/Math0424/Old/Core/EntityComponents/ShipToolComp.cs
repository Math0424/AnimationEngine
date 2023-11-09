using AnimationEngine.Core;
using AnimationEngine.Language;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine
{
    internal class ShipToolComp : EntityComponent
    {
        public Action<SVariable> ToolActivated;

        IMyShipToolBase block;
        bool active;

        public void InitBuilt(CoreScript parent)
        {
            if (parent.Entity is IMyShipToolBase)
            {
                block = parent.Entity as IMyShipToolBase;
                active = !block.IsActivated;
            }
        }

        public void Close() 
        {
            block = null;
        }

        public void Tick(int time)
        {
            if (block == null)
                return;

            if (active != block.IsActivated)
            {
                ToolActivated?.Invoke(new SVariableBool(block.IsActivated));
                active = block.IsActivated;
            }
        }
    }
}
