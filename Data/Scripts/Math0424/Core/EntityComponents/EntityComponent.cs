using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;

namespace AnimationEngine.Core
{
    internal interface EntityComponent
    {

        public void Init(CoreScript parent);

        public void Tick(int time);

        public void Close();

    }
}
