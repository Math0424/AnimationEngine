using AnimationEngine.Language;
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

        public override void Init(SubpartCore core)
        {
            base.Init(core);

            this.core = core;
            OnHover += HoverChange;
            OnUnHover += HoverChange;
            OnInteract += Interacted;

            core.AddMethod("enabled", SetEnabled);
            core.AddMethod("interactable", SetInteractable);
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

        public SVariable SetInteractable(SVariable[] arr)
        {
            this.interactable = arr[0].AsBool();
            return null;
        }

        public SVariable SetEnabled(SVariable[] arr)
        {
            this.enabled = arr[0].AsBool();
            return null;
        }

    }
}
