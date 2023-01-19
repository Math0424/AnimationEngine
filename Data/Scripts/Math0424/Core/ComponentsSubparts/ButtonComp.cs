using Sandbox.Game;
using System;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class ButtonComp : InteractableComp
    {
        public Action ButtonOn;
        public Action ButtonOff;

        private SubpartCore core;
        private bool enabled;

        public ButtonComp(string dummy) : base(dummy)
        {
        }

        public override void Initalize(SubpartCore core)
        {
            base.Initalize(core);

            this.core = core;
            OnHover += HoverChange;
            OnUnHover += HoverChange;
            OnInteract += Interacted;

            core.Actions.Add("enabled", SetEnabled);
            core.Actions.Add("interactable", SetInteractable);
        }

        private void HoverChange()
        {
            if (IsHovering && interactable)
            {
                MyVisualScriptLogicProvider.SetHighlightLocal(core.Subpart.Name, 20, 0, new Color(0, 255, 255) * .3f);
            }
            else
            {
                MyVisualScriptLogicProvider.SetHighlightLocal(core.Subpart.Name, 0);
            }
        }

        private void Interacted()
        {
            MyVisualScriptLogicProvider.PlayHudSoundLocal();
            enabled = !enabled;
            if (enabled)
            {
                ButtonOn?.Invoke();
            } 
            else
            {
                ButtonOff?.Invoke();
            }
        }

        public void SetInteractable(object[] arr)
        {
            this.interactable = (bool)arr[0];
        }

        public void SetEnabled(object[] arr)
        {
            this.enabled = (bool)arr[0];
        }

    }
}
