﻿using AnimationEngine.Core;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Entity;
using static CoreSystems.Api.WcApi;
using static VRage.Game.MyObjectBuilder_Checkpoint;

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
            if (!AnimationEngine.WCApi.IsReady)
            {
                AnimationEngine.WCReady += () => AnimationEngine.WCApi.MonitorEvents(gun, id, triggers);
            }
            else
            {
               AnimationEngine.WCApi.MonitorEvents(gun, id, triggers);
            }
        }

        private void TriggerEvent(int v, bool a)
        {
            if (a)
                parent.Execute($"act_7749_{((WCEventTriggers)v).ToString().ToLower()}");
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

        public ModItem GetMod()
        {
            return parent.GetMod();
        }

        public void InitBuilt(CoreScript core)
        {
            if ((core.Flags & CoreScript.BlockFlags.WeaponcoreInit) == 0)
            {
                core.Flags |= CoreScript.BlockFlags.WeaponcoreInit;
                ((ScriptV2Runner)parent).AddLibrary(new WeaponcoreCore(core, id));
            }
            parent.InitBuilt(core);
        }

        public void Tick(int time)
        {
            parent.Tick(time);
        }
    }
}
