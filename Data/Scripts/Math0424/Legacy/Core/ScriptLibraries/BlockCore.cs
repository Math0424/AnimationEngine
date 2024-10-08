﻿using AnimationEngine.Language;
using Sandbox.Definitions;
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
        public SVariable TranslateTranslateRelative(SVariable[] args)
        {
            args[0] = new SVariableVector(Vector3.Transform(args[0].AsVector3(), quat));
            blockMover.TranslateRelative(args);
            return null;
        }
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
            AddMethod("translate", TranslateTranslateRelative);
            AddMethod("rotate", RotateTranslate);
            AddMethod("rotatearound", RotateAroundTranslate);
            AddMethod("spin", SpinTranslate);
            AddMethod("scale", blockMover.Scale);
            AddMethod("vibrate", blockMover.Vibrate);
            AddMethod("setresetpos", blockMover.SetResetPos);
            AddMethod("resetpos", blockMover.ResetPos);
            AddMethod("resetrot", blockMover.ResetRot);
            AddMethod("reset", blockMover.Reset);

            if (Block is IMyGasTank)
            {
                AddMethod("getgasfilledratio", GetGasFilledRatio);
            }
            if (Block is IMyWarhead)
            {
                AddMethod("detonationtime", DetonationTime);
                AddMethod("iscountingdown", IsCountingDown);
                AddMethod("isarmed", IsArmed);
            }

            //These were a bad idea.
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
            if (Block is IMyDoor)
            {
                AddMethod("opendoor", (e) => { OpenDoor(); return null; });
                AddMethod("closedoor", (e) => { CloseDoor(); return null; });
                AddMethod("toggledoor", (e) => { ToggleDoor(); return null; });
            }

            if (Block is IMyProductionBlock)
            {
                AddMethod("productionitemmodel", GetProductionItemModel);
            }

            AddMethod("currentthrustpercent", CurrentThrustPercent);

            AddMethod("isoccupied", IsOccupied);
            AddMethod("isworking", IsWorking);
            AddMethod("isfunctional", IsFunctional);

            if (Block is IMyTerminalBlock)
            {
                AddMethod("canaccess", CanAccess);
            }
        }

        public override void Tick(int tick)
        {
            pilotMover?.Tick(tick);
            blockMover?.Tick(tick);
        }

        private SVariable GetProductionItemModel(SVariable[] arr)
        {
            if (!((IMyProductionBlock)Block).IsQueueEmpty)
            {
                var id = ((MyBlueprintDefinitionBase)((IMyProductionBlock)Block).GetQueue()[0].Blueprint).Results[0].Id;
                MyPhysicalItemDefinition myPhysicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(id);
                if (myPhysicalItemDefinition != null)
                {
                    return new SVariableString(myPhysicalItemDefinition.Model);
                }
            }
            return null;
        }

        private SVariable DetonationTime(SVariable[] arr)
        {
            return new SVariableFloat(((IMyWarhead)Block).DetonationTime);
        }

        private SVariable IsCountingDown(SVariable[] arr)
        {
            return new SVariableBool(((IMyWarhead)Block).IsCountingDown);
        }

        private SVariable CanAccess(SVariable[] arr)
        {
            return new SVariableBool(((IMyTerminalBlock)Block).HasLocalPlayerAccess());
        }

        private SVariable IsArmed(SVariable[] arr)
        {
            return new SVariableBool(((IMyWarhead)Block).IsArmed);
        }

        private SVariable GetGasFilledRatio(SVariable[] arr)
        {
            return new SVariableFloat((float)((IMyGasTank)Block).FilledRatio);
        }

        private SVariable IsFunctional(SVariable[] arr)
        {
            return new SVariableBool(Block.IsFunctional);
        }

        private SVariable IsWorking(SVariable[] arr)
        {
            return new SVariableBool(Block.IsWorking);
        }

        private SVariable IsOccupied(SVariable[] arr)
        {
            if (Block is IMyShipController)
            {
                return new SVariableBool(((IMyShipController)Block).Pilot is IMyCharacter);
            }
            return null;
        }

        private SVariable CurrentThrustPercent(SVariable[] var)
        {
            if (Block is IMyThrust)
            {
                if (((IMyThrust)Block).MaxEffectiveThrust <= 0)
                    return new SVariableFloat(0);
                else
                    return new SVariableFloat(((IMyThrust)Block).CurrentThrust / ((IMyThrust)Block).MaxEffectiveThrust);
            }
            return null;
        }

        private void ControlAquired()
        {
            if (Block == null || ((IMyCockpit)Block).Pilot == null)
                return;

            pilotMover = new Mover(((IMyCockpit)Block).Pilot.PositionComp);
            pilotMover.AddToScriptLib(this, "pilot");
        }

        private void ControlReleased()
        {
            if (pilotMover == null)
                return;

            pilotMover.Clear();
            pilotMover.RemoveFromScriptLib(this, "pilot");
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
