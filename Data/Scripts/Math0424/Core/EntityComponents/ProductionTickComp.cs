using AnimationEngine.Core;
using Sandbox.Game.EntityComponents;
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
            if (SinkComp != null) {
                foreach (MyDefinitionId resourceType in SinkComp.AcceptedResources)
                    isProducing |= (SinkComp.CurrentInputByType(resourceType) > 0f);
            }
        }

        public void Init(CoreScript parent)
        {
            if (parent.Entity.Components.Has<MyResourceSourceComponent>())
                SourceComp = parent.Entity.Components.Get<MyResourceSourceComponent>();
            if (parent.Entity.Components.Has<MyResourceSinkComponent>())
                SinkComp = parent.Entity.Components.Get<MyResourceSinkComponent>();
        }

        public void Close() { }

        public void Tick(int time)
        {
            if (SourceComp == null && SinkComp == null)
            {
                tick = 0;
                return;
            }

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

            tick += time;
            if (LoopTime != -1 && (tick % LoopTime == 0 || tick > LoopTime))
            {
                tick = 0;
                Ticked?.Invoke();
            }
        }

    }
}
