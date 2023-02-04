using AnimationEngine.Core;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.ModAPI;

namespace AnimationEngine
{
    internal class ProductionTickComp : EntityComponent
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

        public void Init(CoreScript parent)
        {
            if (parent.Entity is IMyProductionBlock)
            {
                productionBlock = ((IMyProductionBlock)parent.Entity);
                productionBlock.StartedProducing += StartedProducing;
                productionBlock.StoppedProducing += StoppedProducing;
            }
            else if (parent.Entity.Components.Has<MyResourceSourceComponent>())
            {
                nonProductionBlock = true;
                SourceComp = parent.Entity.Components.Get<MyResourceSourceComponent>();
            }
        }

        public void Close()
        {
            if (productionBlock != null)
            {
                productionBlock.StartedProducing -= StartedProducing;
                productionBlock.StoppedProducing -= StoppedProducing;
            }
        }

        public void Tick(int time)
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

            tick += time;
            if (tick % LoopTime == 0 || tick > LoopTime)
            {
                tick = 0;
                Ticked?.Invoke();
            }
        }

    }
}
