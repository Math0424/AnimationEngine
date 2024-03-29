﻿using AnimationEngine.Core;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using VRage.Game;

namespace AnimationEngine
{
    internal class ProductionTickComp : EntityComponent
    {
        public Action Ticked;
        public Action StartedProducing;
        public Action StoppedProducing;
        private int tick;
        private int totalTime;
        public int LoopTime;

        private MyResourceSinkComponent SinkComp;
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
            if (SourceComp != null)
            {
                foreach (MyDefinitionId resourceType in SourceComp.ResourceTypes)
                    isProducing |= (SourceComp.CurrentOutputByType(resourceType) > 0f);
            }
            if (SinkComp != null)
            {
                foreach (MyDefinitionId resourceType in SinkComp.AcceptedResources)
                    isProducing |= (SinkComp.CurrentInputByType(resourceType) > 0f);
            }
        }

        //keen hack
        int prevProducingTick = -1;
        private void CallProducing()
        {
            if (prevProducingTick != totalTime)
            {
                isProducing = true;
                prevProducingTick = totalTime;
                StartedProducing?.Invoke();
            }
        }

        //more keen hackery
        int prevStopProducingTick = -1;
        private void CallStopProducing()
        {
            if (prevStopProducingTick != totalTime)
            {
                isProducing = false;
                prevStopProducingTick = totalTime;
                StoppedProducing?.Invoke();
            }
        }

        public void InitBuilt(CoreScript parent)
        {
            if (parent.Entity is IMyProductionBlock)
            {
                (parent.Entity as IMyProductionBlock).StartedProducing -= CallProducing;
                (parent.Entity as IMyProductionBlock).StoppedProducing -= CallStopProducing;

                (parent.Entity as IMyProductionBlock).StartedProducing += CallProducing;
                (parent.Entity as IMyProductionBlock).StoppedProducing += CallStopProducing;
            }
            else
            {
                if (parent.Entity.Components.Has<MyResourceSourceComponent>())
                    SourceComp = parent.Entity.Components.Get<MyResourceSourceComponent>();
                if (parent.Entity.Components.Has<MyResourceSinkComponent>())
                    SinkComp = parent.Entity.Components.Get<MyResourceSinkComponent>();
            }
            isProducing = false;
        }

        public void Close() { }

        public void Tick(int time)
        {
            totalTime++;

            if (SourceComp != null || SinkComp != null)
            {
                bool previousState = isProducing;
                UpdateProducingState();

                if (previousState != isProducing)
                {
                    if (isProducing)
                    {
                        StartedProducing?.Invoke();
                    }
                    else
                    {
                        StoppedProducing?.Invoke();
                    }
                }
            }

            if (!isProducing)
                return;

            tick += time;
            if (LoopTime != -1 && (tick % LoopTime == 0 || tick > LoopTime))
            {
                tick = 0;
                Ticked?.Invoke();
            }
        }

    }
}
