using AnimationEngine.Core;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;

namespace AnimationEngine
{
    internal class CockpitComp : EntityComponent
    {
        public Action EnteredSeat;
        public Action ExitedSeat;

        private bool enteredSeat = false;
        private IMyCockpit block;

        public void InitBuilt(CoreScript parent)
        {
            if (parent.Entity is IMyCockpit)
            {
                this.block = (IMyCockpit)parent.Entity;

                this.block.ControllerInfo.ControlReleased -= Released;
                this.block.ControllerInfo.ControlAcquired -= Controlled;
                this.block.ControllerInfo.ControlReleased += Released;
                this.block.ControllerInfo.ControlAcquired += Controlled;

                if (block.Pilot is IMyCharacter)
                    EnteredSeat?.Invoke();
            }
        }

        public void Close()
        {
            this.block.ControllerInfo.ControlReleased -= Released;
            this.block.ControllerInfo.ControlAcquired -= Controlled;
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

        public void Tick(int time)
        {

        }
    }
}
