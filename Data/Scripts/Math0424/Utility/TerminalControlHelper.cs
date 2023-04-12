using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;

namespace AnimationEngine.Utility
{
    internal class TerminalControlHelper
    {
        private static Dictionary<long, Action> _pushedButton;
        private static Dictionary<long, Action<bool>> _pushedOnOffToggle;


        private static void OnOffToggled(IMyTerminalBlock block, bool value)
        {
            if (_pushedOnOffToggle.ContainsKey(block.EntityId))
            {
                _pushedOnOffToggle[block.EntityId].Invoke(value);
            }
        }

        private static void ButtonPushed(IMyTerminalBlock block)
        {
            if (_pushedButton.ContainsKey(block.EntityId))
            {
                _pushedButton[block.EntityId].Invoke();
            }
        }

        private static bool VisibleControl(IMyTerminalBlock block)
        {
            return true;
        }


        public static void CreateOnOffTerminal(string subtypeid, int position, string title, string tooltip)
        {
            var control = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>($"{subtypeid}_{title}_{position}_onoffswitch_ae");
            control.Title = MyStringId.GetOrCompute(title);
            control.Tooltip = MyStringId.GetOrCompute(tooltip);
            control.SupportsMultipleBlocks = true;
            control.Setter += OnOffToggled;
            control.Visible += VisibleControl;
            SetPosition<IMyTerminalBlock>(position, control);
        }

        public static void CreateButtonTerminal(string subtypeid, int position, string title, string tooltip)
        {
            var control = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>($"{subtypeid}_{title}_{position}_button_ae");
            control.Title = MyStringId.GetOrCompute(title);
            control.Tooltip = MyStringId.GetOrCompute(tooltip);
            control.SupportsMultipleBlocks = true;
            
            control.Action += ButtonPushed;
            control.Enabled += VisibleControl;
            control.Visible += VisibleControl;

            MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(control);
            //SetPosition<IMyTerminalBlock>(position, control);
        }

        private static void SetPosition<T>(int pos, IMyTerminalControl control)
        {
            List<IMyTerminalControl> controls;
            MyAPIGateway.TerminalControls.GetControls<T>(out controls);

            if (pos >= controls.Count)
            {
                MyAPIGateway.TerminalControls.AddControl<T>(control);
                return;
            }

            foreach (var x in controls)
                MyAPIGateway.TerminalControls.RemoveControl<T>(x);
            for(int i = 0; i < controls.Count; i++)
            {
                if (i == pos)
                    MyAPIGateway.TerminalControls.AddControl<T>(control);
                MyAPIGateway.TerminalControls.AddControl<T>(controls[i]);
            }
        }
    }
}
