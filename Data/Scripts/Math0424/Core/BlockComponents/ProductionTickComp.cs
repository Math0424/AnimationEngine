using AnimationEngine.Core;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.ModAPI;

namespace AnimationEngine
{
    internal class ProductionTickComp : BlockComponent
    {
        public Action Ticked;
        public Action StartedProducing;
        public Action StoppedProducing;
        private int tick;
        public int LoopTime;

        private bool nonProductionBlock = false;
        private IMyProductionBlock productionBlock;
        private MyResourceSourceComponent SourceComp;
        private bool isProducing = false;

        public ProductionTickComp(int loop)
        {
            this.LoopTime = loop;
            tick = -1;
        }

        private void UpdateProducingState()
        {
            isProducing = false;
            foreach (MyDefinitionId resourceType in SourceComp.ResourceTypes)
            {
                isProducing |= (SourceComp.CurrentOutputByType(resourceType) > 0f);
            }
        }

        public override void Initalize(IMyCubeBlock block)
        {
            if (block is IMyProductionBlock)
            {
                productionBlock = ((IMyProductionBlock)block);
                productionBlock.StartedProducing += StartedProducing;
                productionBlock.StoppedProducing += StoppedProducing;
            } 
            else if(block.Components.Has<MyResourceSourceComponent>())
            {
                nonProductionBlock = true;
                SourceComp = block.Components.Get<MyResourceSourceComponent>();
            }
        }

        public override void Tick(int i)
        {
            if (LoopTime == -1 || (!nonProductionBlock && !productionBlock.IsProducing))
            {
                tick = 0;
                return;
            }

            if (nonProductionBlock)
            {
                bool previousState = isProducing;
                UpdateProducingState();
                
                if (previousState != isProducing)
                {
                    if (isProducing)
                        StartedProducing?.Invoke();
                    else
                        StoppedProducing?.Invoke();
                }
                
                if (!isProducing)
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
