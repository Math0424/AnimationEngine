using AnimationEngine.Core;
using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    internal class WorkingTickComp : EntityComponent
    {
        public Action Ticked;
        public Action OnIsWorking;
        public Action OnNotWorking;
        private int tick;
        private Vector3 prevColor = Vector3.Zero;
        public int LoopTime;
        IMyCubeBlock block;

        public WorkingTickComp(int loop)
        {
            this.LoopTime = loop;
            tick = -1;
        }

        private void WorkingChange(IMyCubeBlock e)
        {
            if (e.IsWorking) 
                OnIsWorking?.Invoke(); 
            else 
                OnNotWorking?.Invoke(); 
        }

        public void Init(CoreScript parent)
        {
            if (parent.Entity is IMyCubeBlock)
            {
                this.block = (IMyCubeBlock)parent.Entity;

                block.IsWorkingChanged -= WorkingChange;
                block.IsWorkingChanged += WorkingChange;

                prevColor = block.Render.ColorMaskHsv;
                WorkingChange(block);
            }
        }

        public void Close()
        {
            //todo cleanup the states
        }

        public void Tick(int i)
        {
            if (block == null || !block.IsWorking)
            {
                return;
            }

            if (prevColor != block.Render.ColorMaskHsv)
            {
                prevColor = block.Render.ColorMaskHsv;
                WorkingChange(block);
            }

            if (LoopTime == -1)
            {
                return;
            }

            tick += i;
            if (tick % LoopTime == 0 || tick > LoopTime)
            {
                tick = 0;
                Ticked?.Invoke();
            }
        }

    }
}
