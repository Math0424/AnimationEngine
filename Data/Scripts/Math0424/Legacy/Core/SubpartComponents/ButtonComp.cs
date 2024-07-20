using AnimationEngine.Language;
using AnimationEngine.Utility;
using Math0424.Networking;
using ProtoBuf;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class ButtonComp : InteractableComp
    {
        private static Dictionary<string, Action> registeredButtons = new Dictionary<string, Action>();

        public static void ServerButtonIn(Type t, PacketIn p)
        {
            if (t == typeof(ButtonPacket))
            {
                var id = p.UnWrap<ButtonPacket>().id;
                if (registeredButtons.ContainsKey(id))
                    registeredButtons[id].Invoke();
            }
        }

        public static void ButtonIn(PacketIn p)
        {
            if (p.IsFromServer)
            {
                var id = p.UnWrap<ButtonPacket>().id;
                if (registeredButtons.ContainsKey(id))
                    registeredButtons[id].Invoke();
            }
        }

        [ProtoContract] public struct ButtonPacket {
            [ProtoMember(1)] public string id;
        }

        public Action<SVariable> Hovering;
        public Action<SVariable> Pressed;

        private SubpartCore core;
        private bool enabled;

        private string registeredId;

        public ButtonComp(string dummy) : base(dummy) 
        {
            OnHover += HoverChange;
            OnHover += HoverScriptInvoke;

            OnInteract += Interacted;
            if (!MyAPIGateway.Utilities.IsDedicated)
                OnInteract += SyncInteraction;
        }

        public override void Init(SubpartCore core)
        {
            base.Init(core);

            this.core = core;

            core.AddMethod("enabled", SetEnabled);
            core.AddMethod("interactable", SetInteractable);

            registeredId = $"{dummy}:{core?.Subpart?.EntityId ?? 0}:{core?.Subpart?.Parent?.EntityId ?? 0}";
            registeredButtons.Add(registeredId, Interacted);
        }

        public override void Close()
        {
            base.Close();

            Hovering?.UnSubscribeAll();
            Pressed?.UnSubscribeAll();

            if (registeredId != null)
                registeredButtons.Remove(registeredId);
        }

        private void HoverScriptInvoke(bool b)
        {
            Hovering?.Invoke(new SVariableBool(b));
        }

        private void HoverChange(bool v)
        {
            if (v && interactable)
            {
                var color = MyDefinitionManager.Static.EnvironmentDefinition.ContourHighlightColor;
                MyVisualScriptLogicProvider.SetHighlightLocal(core.Subpart.Name, 9, 3, color);
            }
            else
                MyVisualScriptLogicProvider.SetHighlightLocal(core.Subpart.Name, -1);
        }

        private void SyncInteraction()
        {
            EasyNetworker.SendToSyncRange(new ButtonPacket { id = registeredId }, EasyNetworker.TransitType.ExcludeSender);
        }

        private void Interacted()
        {
            MyVisualScriptLogicProvider.PlayHudSoundLocal();
            enabled = !enabled;
            Pressed?.Invoke(new SVariableBool(enabled));
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
