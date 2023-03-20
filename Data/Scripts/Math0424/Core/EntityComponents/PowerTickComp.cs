using AnimationEngine.Core;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using VRage.Game;

namespace AnimationEngine
{
    internal class PowerTickComp : EntityComponent
    {
        public Action<float> Consumed;
        public Action<float> Produced;

        public int currentTick;
        private MyResourceSinkComponent SinkComp;
        private MyResourceSourceComponent SourceComp;

        public PowerTickComp() { }

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
                return;

            currentTick -= time;
            if (currentTick <= 0)
            {
                currentTick = 30;

                if (SourceComp != null)
                {
                    foreach (MyDefinitionId resourceType in SourceComp.ResourceTypes)
                        Produced?.Invoke(SourceComp.CurrentOutputByType(resourceType) / SourceComp.MaxOutputByType(resourceType));
                }

                if (SinkComp != null)
                {
                    foreach (MyDefinitionId resourceType in SinkComp.AcceptedResources)
                        Consumed?.Invoke(SinkComp.CurrentInputByType(resourceType) / SinkComp.MaxRequiredInputByType(resourceType));
                }
            }
        }

    }
}
