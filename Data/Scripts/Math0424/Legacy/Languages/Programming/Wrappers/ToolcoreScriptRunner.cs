using AnimationEngine.Core;
using System;
using ToolCore.API;
using VRage.Game.Entity;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Language
{
    internal class ToolcoreScriptRunner : ScriptRunner
    {
        ScriptRunner parent;
        Action<int, bool> triggers;
        MyEntity gun;
        int id;

        public ToolcoreScriptRunner(int id, ScriptRunner parent)
        {
            this.id = id;
            this.parent = parent;
            triggers += TriggerEvent;
        }

        public void ListenToEvents(MyEntity entity)
        {
            this.gun = entity;
            if (!AnimationEngine.TCApi.IsReady)
            {
                AnimationEngine.TCReady += () => AnimationEngine.WCApi.MonitorEvents(entity, id, triggers);
            }
            else
            {
               AnimationEngine.TCApi.MonitorEvents(entity, triggers);
            }
        }

        private void TriggerEvent(int v, bool a)
        {
            if (a)
                parent.Execute($"act_7750_{((ToolCoreEnum)v).ToString().ToLower()}");
        }

        public void Stop()
        {
            parent.Stop();
        }

        public ScriptRunner Clone()
        {
            return new ToolcoreScriptRunner(id, parent.Clone());
        }

        public void Close()
        {
            triggers -= TriggerEvent;
            if (gun != null)
                AnimationEngine.TCApi.UnMonitorEvents(gun, triggers);
            parent.Close();
        }

        public void Execute(string function, params SVariable[] args)
        {
            parent.Execute(function, args);
        }

        public ModItem GetMod()
        {
            return parent.GetMod();
        }

        public void InitBuilt(CoreScript core)
        {
            parent.InitBuilt(core);
        }

        public void Tick(int time)
        {
            parent.Tick(time);
        }
    }
}
