using AnimationEngine.Language;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class BlockCore : ScriptLib
    {
        private IMyCubeBlock Block;
        Mover pilotMover;
        Mover blockMover;
        Quaternion quat;

        //translate([x, y, z], time, lerp)
        public SVariable TranslateTranslate(SVariable[] args)
        {
            args[0] = new SVariableVector(Vector3.Transform(args[0].AsVector3(), quat));
            blockMover.Translate(args);
            return null;
        }
        //rotate([x, y, z], angle, time, lerp)
        public SVariable RotateTranslate(params SVariable[] args)
        {
            args[0] = new SVariableVector(Vector3.Transform(args[0].AsVector3(), quat));
            blockMover.Rotate(args);
            return null;
        }

        //rotate([x, y, z], [x, y, z] pivot angle, time, lerp)
        public SVariable RotateAroundTranslate(SVariable[] args)
        {
            args[0] = new SVariableVector(Vector3.Transform(args[0].AsVector3(), quat));
            args[1] = new SVariableVector(Vector3.Transform(args[1].AsVector3(), quat));
            blockMover.RotateAround(args);
            return null;
        }

        //spin([x, y, z], speed, time)
        public SVariable SpinTranslate(SVariable[] args)
        {
            args[0] = new SVariableVector(Vector3.Transform(args[0].AsVector3(), quat));
            blockMover.Spin(args);
            return null;
        }

        public BlockCore(CoreScript script)
        {
            if (!(script.Entity is IMyCubeBlock))
            {
                return;
            }
            this.Block = script.Entity as IMyCubeBlock;

            if (Block is IMyDoor)
            {
                AddMethod("opendoor", (e) => { OpenDoor(); return null; });
                AddMethod("closedoor", (e) => { CloseDoor(); return null; });
                AddMethod("toggledoor", (e) => { ToggleDoor(); return null; });
            }

            if (Block is IMyCockpit)
            {
                var comp = script.GetFirstComponent<CockpitComp>();
                if (comp != null)
                {
                    comp.EnteredSeat += ControlAquired;
                    comp.ExitedSeat += ControlReleased;
                }
            }

            Block.Orientation.GetQuaternion(out quat);
            blockMover = new Mover(Block.PositionComp);
            AddMethod("translate", TranslateTranslate);
            AddMethod("rotate", RotateTranslate);
            AddMethod("rotatearound", RotateAroundTranslate);
            AddMethod("spin", SpinTranslate);
            AddMethod("vibrate", blockMover.Vibrate);
            AddMethod("reset", blockMover.Reset);
            AddMethod("resetpos", blockMover.ResetPos);
            AddMethod("setresetpos", blockMover.SetResetPos);

            if (Block is IMyLandingGear)
            {
                AddMethod("lockon", (e) => { LockOn(); return null; });
                AddMethod("lockoff", (e) => { LockOff(); return null; });
                AddMethod("togglelock", (e) => { ToggleLock(); return null; });
            }

            if (Block is IMyFunctionalBlock)
            {
                AddMethod("poweron", (e) => { PowerOn(); return null; });
                AddMethod("poweroff", (e) => { PowerOff(); return null; });
                AddMethod("togglepower", (e) => { TogglePower(); return null; });
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
                return;

            pilotMover = new Mover(((IMyCockpit)Block).Pilot.PositionComp);
            AddMethod("pilottranslate", pilotMover.Translate);
            AddMethod("pilotrotate", pilotMover.Rotate);
            AddMethod("pilotrotatearound", pilotMover.RotateAround);
            AddMethod("pilotspin", pilotMover.Spin);
            AddMethod("pilotvibrate", pilotMover.Vibrate);
            AddMethod("pilotreset", pilotMover.Reset);
            AddMethod("pilotresetpos", pilotMover.ResetPos);
            AddMethod("pilotsetresetpos", pilotMover.SetResetPos);
        }

        private void ControlReleased()
        {
            if (pilotMover == null)
                return;

            pilotMover.Clear();
            RemoveMethod("pilottranslate");
            RemoveMethod("pilotrotate");
            RemoveMethod("pilotrotatearound");
            RemoveMethod("pilotspin");
            RemoveMethod("pilotvibrate");
            RemoveMethod("pilotreset");
            RemoveMethod("pilotresetpos");
            RemoveMethod("pilotsetresetpos");
        }

        private void PowerOff()
        {
            ((IMyFunctionalBlock)Block).Enabled = false;
        }
        private void PowerOn()
        {
            ((IMyFunctionalBlock)Block).Enabled = true;
        }
        private void TogglePower()
        {
            ((IMyFunctionalBlock)Block).Enabled = !((IMyFunctionalBlock)Block).Enabled;
        }

        private void OpenDoor()
        {
            (Block as IMyDoor).OpenDoor();
        }
        private void CloseDoor()
        {
            (Block as IMyDoor).CloseDoor();
        }
        private void ToggleDoor()
        {
            (Block as IMyDoor).ToggleDoor();
        }

        private void LockOn()
        {
            (Block as IMyLandingGear).Lock();
        }
        private void LockOff()
        {
            (Block as IMyLandingGear).Unlock();
        }
        private void ToggleLock()
        {
            (Block as IMyLandingGear).ToggleLock();
        }

    }
}
