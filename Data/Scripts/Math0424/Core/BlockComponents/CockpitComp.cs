using AnimationEngine.Core;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace AnimationEngine
{
    internal class CockpitComp : BlockComponent
    {
        public Action EnteredSeat;
        public Action ExitedSeat;

        private bool enteredSeat = false;
        private IMyCockpit block;
        public override void Initalize(IMyCubeBlock block)
        {
            this.block = (IMyCockpit)block;
            this.block.ControllerInfo.ControlReleased += Released;
            this.block.ControllerInfo.ControlAcquired += Controlled;
        }

        private void Controlled(IMyEntityController con)
        {
            if (enteredSeat)
            {
                return;
            }
            EnteredSeat?.Invoke();
            enteredSeat = true;
        }

        private void Released(IMyEntityController con)
        {
            if (block.Pilot is IMyCharacter)
            {
                return;
            }
            ExitedSeat?.Invoke();
            enteredSeat = false;
        }

        public override void Tick(int i)
        {

        }
    }
}
