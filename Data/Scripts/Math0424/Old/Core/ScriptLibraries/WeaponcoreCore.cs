using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System.Linq;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class WeaponcoreCore : ScriptLib
    {
        int WeaponId;
        private MyEntity gun;

        public WeaponcoreCore(CoreScript script, int weaponId)
        {
            if (!AnimationEngine.WCApi.IsReady || !(script.Entity is IMyCubeBlock))
            {
                return;
            }
            WeaponId = weaponId;
            gun = (MyEntity)script.Entity;

            AddMethod("getactiveammo", GetActiveAmmo);
            AddMethod("getheatlevel", GetHeatLevel);
            AddMethod("getshotsfired", GetShotsFired);
            AddMethod("isshooting", IsShooting);
        }

        public override void Tick(int tick)
        {
            
        }

        private SVariable GetActiveAmmo(SVariable[] arr)
        {
            return new SVariableString(AnimationEngine.WCApi.GetActiveAmmo(gun, WeaponId));
        }

        private SVariable GetHeatLevel(SVariable[] arr)
        {
            return new SVariableFloat(AnimationEngine.WCApi.GetHeatLevel(gun));
        }

        private SVariable GetShotsFired(SVariable[] arr)
        {
            return new SVariableInt(AnimationEngine.WCApi.GetShotsFired(gun, WeaponId));
        }

        private SVariable IsShooting(SVariable[] arr)
        {
            return new SVariableBool(AnimationEngine.WCApi.IsWeaponShooting(gun, WeaponId));
        }

    }
}
