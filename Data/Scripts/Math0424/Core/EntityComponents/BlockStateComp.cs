using AnimationEngine.Core;
using AnimationEngine.Utility;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    internal class BlockStateComp : EntityComponent
    {

        public Action Create;
        public Action Built;
        public Action Damaged;

        // powered and functional
        public Action Working;
        // power turned off
        public Action NotWorking;

        private CoreScript core;
        private IMyCubeBlock block;
        private bool isFunctional = true;
        private bool isWorking = true;
        private Vector3 prevColor = Vector3.Zero;

        void EntityComponent.Close()
        {

        }

        private void WorkingChange(IMyCubeBlock e)
        {
            if (!e.IsWorking)
                NotWorking?.Invoke();
        }

        void EntityComponent.InitBuilt(CoreScript parent)
        {
            this.core = parent;
            if (core.Entity is IMyCubeBlock)
            {
                block = core.Entity as IMyCubeBlock;

                block.IsWorkingChanged -= WorkingChange;
                block.IsWorkingChanged += WorkingChange;

                isWorking = !block.IsWorking;
            }
        }

        void EntityComponent.Tick(int time)
        {
            if (block == null)
                return;

            if ((core.Flags & CoreScript.BlockFlags.Created) != 0)
            {
                core.Flags &= ~CoreScript.BlockFlags.Created;
                Create?.Invoke();
            }

            if ((core.Flags & CoreScript.BlockFlags.Built) != 0)
            {
                core.Flags &= ~CoreScript.BlockFlags.Built;
                Built?.Invoke();
            }

            if (prevColor != block.Render.ColorMaskHsv)
            {
                prevColor = block.Render.ColorMaskHsv;
                isWorking = !block.IsWorking;
            }

            if (isWorking != block.IsWorking)
            {
                isWorking = block.IsWorking;
                if (isWorking)
                    Working?.Invoke();
            }

            if (isFunctional != block.IsFunctional)
            {
                isFunctional = block.IsFunctional;
                if (!isFunctional)
                    Damaged?.Invoke();
            }
        }
    }
}
