using Sandbox.Game.Components;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;
using System;
using VRageMath;
using VRage.Game.Entity;
using AnimationEngine.Util;

namespace AnimationEngine.Core
{
    internal class BlockCore : Actionable
    {
        private IMyCubeBlock Block;
        Mover pilotMover;
        Mover blockMover;

        public BlockCore(BlockScript script)
        {
            this.Block = script.Block;

            Actions.Add("log", Log);

            if (Block is IMyDoor)
            {
                Actions.Add("opendoor", OpenDoor);
                Actions.Add("closedoor", CloseDoor);
                Actions.Add("toggledoor", ToggleDoor);
            }

            if (Block is IMyCockpit)
            {
                var comp = script.GetComponent<CockpitComp>();
                if (comp != null)
                {
                    comp.EnteredSeat += ControlAquired;
                    comp.ExitedSeat += ControlReleased;
                }
            }

            blockMover = new Mover(Block.PositionComp);
            Actions.Add("translate", blockMover.Translate);
            Actions.Add("rotate", blockMover.Rotate);
            Actions.Add("rotatearound", blockMover.RotateAround);
            Actions.Add("spin", blockMover.Spin);
            Actions.Add("vibrate", blockMover.Vibrate);

            if (Block is IMyLandingGear)
            {
                Actions.Add("lockon", LockOn);
                Actions.Add("lockoff", LockOff);
                Actions.Add("togglelock", ToggleLock);
            }

            if (Block is IMyFunctionalBlock)
            {
                Actions.Add("poweron", PowerOn);
                Actions.Add("poweroff", PowerOff);
                Actions.Add("togglepower", TogglePower);
            }
        }

        public override void Tick(int tick)
        {
            pilotMover?.Tick(tick);
            blockMover?.Tick(tick);
        }

        private void ControlAquired()
        {
            if (Block == null || ((IMyCockpit)Block).Pilot == null)
            {
                return;
            }

            pilotMover = new Mover(((IMyCockpit)Block).Pilot.PositionComp);
            Actions["pilottranslate"] = pilotMover.Translate;
            Actions["pilotrotate"] = pilotMover.Rotate;
            Actions["pilotrotatearound"] = pilotMover.RotateAround;
            Actions["pilotspin"] = pilotMover.Spin;
            Actions["pilotvibrate"] = pilotMover.Vibrate;
        }

        private void ControlReleased()
        {
            if (pilotMover == null)
                return;

            pilotMover.Clear();
            Actions.Remove("pilottranslate");
            Actions.Remove("pilotrotate");
            Actions.Remove("pilotrotatearound");
            Actions.Remove("pilotspin");
            Actions.Remove("pilotvibrate");
        }

        private void Log(object[] args)
        {
            MyLog.Default.WriteLine($"{Block.EntityId}: {args[0]}");
        }

        private void PowerOff(object[] args)
        {
            ((IMyFunctionalBlock)Block).Enabled = false;
        }
        private void PowerOn(object[] args)
        {
            ((IMyFunctionalBlock)Block).Enabled = true;
        }
        private void TogglePower(object[] args)
        {
            ((IMyFunctionalBlock)Block).Enabled = !((IMyFunctionalBlock)Block).Enabled;
        }

        private void OpenDoor(object[] args)
        {
            (Block as IMyDoor).OpenDoor();
        }
        private void CloseDoor(object[] args)
        {
            (Block as IMyDoor).CloseDoor();
        }
        private void ToggleDoor(object[] args)
        {
            (Block as IMyDoor).ToggleDoor();
        }

        private void LockOn(object[] args)
        {
            (Block as IMyLandingGear).Lock();
        }
        private void LockOff(object[] args)
        {
            (Block as IMyLandingGear).Unlock();
        }
        private void ToggleLock(object[] args)
        {
            (Block as IMyLandingGear).ToggleLock();
        }

    }
}
