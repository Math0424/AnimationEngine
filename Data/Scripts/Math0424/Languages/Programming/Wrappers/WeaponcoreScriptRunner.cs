using AnimationEngine.Core;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Entity;
using static CoreSystems.Api.WcApi;

namespace AnimationEngine.Language
{
    internal class WeaponcoreScriptRunner : ScriptRunner
    {
        ScriptRunner parent;
        Action<int, bool> triggers;
        MyEntity gun;
        int id;

        public WeaponcoreScriptRunner(int id, ScriptRunner parent)
        {
            this.id = id;
            this.parent = parent;
            triggers += TriggerEvent;
        }

        public void ListenToEvents(MyEntity gun)
        {
            this.gun = gun;
            AnimationEngine.WCApi.MonitorEvents(gun, id, triggers);
        }

        private void TriggerEvent(int v, bool a)
        {
            parent.Execute($"act_7749_{((EventTriggers)v).ToString().ToLower()}");
        }

        public void Stop()
        {
            parent.Stop();
        }

        public ScriptRunner Clone()
        {
            return new WeaponcoreScriptRunner(id, parent.Clone());
        }

        public void Close()
        {
            triggers -= TriggerEvent;
            if (gun != null)
                AnimationEngine.WCApi.UnMonitorEvents(gun, id, triggers);
            parent.Close();
        }

        public void Execute(string function, params SVariable[] args)
        {
            parent.Execute(function, args);
        }

        public string GetModName()
        {
            return parent.GetModName();
        }

        public void Init(CoreScript a)
        {
            parent.Init(a);
        }

        public void Tick(int time)
        {
            parent.Tick(time);
        }
    }
}
