using AnimationEngine.Core;
using AnimationEngine.Utility;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    internal class InteractableComp : SubpartComponent
    {

        public Action<bool> OnHover;
        public Action OnInteract;

        private bool IsHovering;
        protected string dummy;
        protected bool interactable;
        private IMyCubeBlock block;

        public InteractableComp(string dummy)
        {
            this.dummy = dummy;
            interactable = true;
        }

        public override void Init(SubpartCore core)
        {
            //TODO fix this for parenting
            //Utils.LogToFile("Part core id " + core.Subpart.Parent);
            block = core?.Subpart?.Parent as IMyCubeBlock;

            //if (block == null || MyAPIGateway.Utilities.IsDedicated)
            //    return;
            //
            //Dictionary<string, IMyModelDummy> dummies = new Dictionary<string, IMyModelDummy>();
            //block.Model.GetDummies(dummies);
            //foreach (var dum in dummies)
            //{
            //    if (dum.Value.Name.Equals(dummy))
            //    {
            //        return;
            //    }
            //}
            //Utils.LogToFile($"Interactable failed to spawn, could not find dummy '{dummy}' valid dummies below");
            //foreach(var dum in dummies)
            //    Utils.LogToFile($" - '{dum.Value.Name}'");
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
