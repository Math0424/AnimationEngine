using AnimationEngine.Core;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine
{
    internal class ThrustComp : EntityComponent
    {
        public Action<float> ThrustChanged;
        IMyThrust thruster;

        public void Close()
        {
            thruster = null;
        }

        public void Init(CoreScript parent)
        {
            if (parent.Entity is IMyThrust)
            {
                thruster = parent.Entity as IMyThrust;
            }
        }

        public void Tick(int time)
        {
            if (thruster != null)
            {
                ThrustChanged?.Invoke(thruster.CurrentThrust / thruster.MaxEffectiveThrust);
            }
        }
    }
}
