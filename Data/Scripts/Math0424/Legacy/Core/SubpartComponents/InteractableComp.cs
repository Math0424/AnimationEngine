using AnimationEngine.Core;
using AnimationEngine.Utility;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    internal class InteractableComp : SubpartComponent
    {

        protected Action<bool> OnHover;
        protected Action OnInteract;

        private bool IsHovering;
        protected string dummy;
        protected bool interactable;
        private IMyCubeBlock block;

        public InteractableComp(string dummy)
        {
            this.dummy = dummy;
            interactable = true;
        }

        public override void Close()
        {
            if (IsHovering)
            {
                IsHovering = false;
                OnHover?.Invoke(false);
            }
        }

        public override void Init(SubpartCore core)
        {
            block = core?.Subpart?.Parent as IMyCubeBlock;
        }

        public override void Tick(int i)
        {
            if (block != null && MyAPIGateway.Session?.Player?.Character != null &&
                Vector3.DistanceSquared(MyAPIGateway.Session.Player.Character.GetPosition(), block.GetPosition()) < 50)
            {
                var view = MyAPIGateway.Session.Camera.WorldMatrix;
                var target = view.Translation + view.Forward * 5;

                var hit = block.MyRaycastDetectors(view.Translation, target);
                if (interactable && hit.Count != 0)
                {
                    if (hit.Contains(dummy))
                    {
                        if (!IsHovering)
                            OnHover?.Invoke(true);
                        IsHovering = true;
                        
                        if (!MyAPIGateway.Gui.IsCursorVisible && !MyAPIGateway.Gui.ChatEntryVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None)
                        {
                            if (MyAPIGateway.Input.IsNewLeftMousePressed() || MyAPIGateway.Input.IsNewGameControlPressed(MyControlsSpace.USE))
                            {
                                OnInteract?.Invoke();
                            }
                        }
                    }
                }
                else if (IsHovering)
                {
                    IsHovering = false;
                    OnHover?.Invoke(false);
                }
            }
        }

    }
}
