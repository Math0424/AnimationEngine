using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class GridCore : ScriptLib
    {
        private IMyCubeGrid Grid;
        private bool UpdateH2;
        private bool UpdateO2;
        private float H2Amount = 0;
        private float O2Amount = 0;
        MyDefinitionId HydrogenId = MyDefinitionId.Parse("MyObjectBuilder_GasProperties/Hydrogen");
        MyDefinitionId OxygenId = MyDefinitionId.Parse("MyObjectBuilder_GasProperties/Oxygen");

        public GridCore(CoreScript script)
        {
            if (!(script.Entity is IMyCubeBlock))
                return;

            //potential bug with grid splitting?
            Grid = ((IMyCubeBlock)script.Entity).CubeGrid;

            AddMethod("isnpc", IsNPC);
            AddMethod("getatmosphericdensity", AtmosphericDensity);
            AddMethod("getplanetaltitude", Altitude);
            AddMethod("getplanetgroundaltitude", GroundAltitude);
            AddMethod("getplanetmaxaltitude", PlanetMaxAltitude);

            AddMethod("getspeed", Speed);
            AddMethod("getnaturalgravity", NaturalGravity);

            AddMethod("geth2fuel", H2Fuel);
            AddMethod("geto2fuel", O2Fuel);

            AddMethod("getfuel", GetFuel);
        }

        public override void Tick(int tick)
        {
            if (Grid != null && (UpdateH2 || UpdateO2))
            {
                UpdateH2 = false;
                UpdateO2 = false;

                var gas = Grid.GetFatBlocks<IMyGasTank>();

                float totalH2Capacity = 0f;
                float totalO2Capacity = 0f;
                float currentH2Capacity = 0f;
                float currentO2Capacity = 0f;
                foreach (IMyGasTank myGasTank in gas)
                {
                    var comp = myGasTank.Components.Get<MyResourceSinkComponent>();
                    if (comp != null)
                    {
                        double filledRatio = myGasTank.FilledRatio;
                        float gasCapacity = myGasTank.Capacity;
                        
                        if (comp.AcceptedResources.Contains(HydrogenId))
                        {
                            totalH2Capacity += gasCapacity;
                            currentH2Capacity += (float)(filledRatio * gasCapacity);
                        } 
                        else if(comp.AcceptedResources.Contains(OxygenId))
                        {
                            totalO2Capacity += gasCapacity;
                            currentO2Capacity += (float)(filledRatio * gasCapacity);
                        }
                    }
                }
                H2Amount = totalH2Capacity == 0 ? 0 : currentH2Capacity / totalH2Capacity;
                O2Amount = totalO2Capacity == 0 ? 0 : currentO2Capacity / totalO2Capacity;
            }
        }

        private SVariable H2Fuel(SVariable[] arr)
        {
            UpdateH2 = true;
            return new SVariableFloat(H2Amount);
        }

        private SVariable O2Fuel(SVariable[] arr)
        {
            UpdateO2 = true;
            return new SVariableFloat(O2Amount);
        }

        //far slower
        private SVariable GetFuel(SVariable[] arr)
        {
            MyDefinitionId id = MyDefinitionId.Parse(arr[0].ToString());
            var gas = Grid.GetFatBlocks<IMyGasTank>();

            float maxGasCapacity = 0f;
            float currentGasCapcity = 0f;
            foreach (IMyGasTank myGasTank in gas)
            {
                var comp = myGasTank.Components.Get<MyResourceSinkComponent>();

                double filledRatio = myGasTank.FilledRatio;
                float gasCapacity = myGasTank.Capacity;

                if (comp.AcceptedResources.Contains(id))
                {
                    maxGasCapacity += gasCapacity;
                    currentGasCapcity += (float)(filledRatio * gasCapacity);
                }
            }

            return new SVariableFloat(maxGasCapacity == 0 ? 0 : currentGasCapcity / maxGasCapacity);
        }

        private SVariable NaturalGravity(SVariable[] arr)
        {
            return new SVariableVector(Grid.NaturalGravity);
        }

        private SVariable PlanetMaxAltitude(SVariable[] arr)
        {
            var planet = MyGamePruningStructure.GetClosestPlanet(Grid.GetPosition());
            if (planet != null)
            {
                float val = planet.AtmosphereRadius - planet.AverageRadius;
                return new SVariableFloat(val);
            }
            return new SVariableFloat(0);
        }

        private SVariable AtmosphericDensity(SVariable[] arr)
        {
            float? val = MyGamePruningStructure.GetClosestPlanet(Grid.PositionComp.GetPosition())?.GetAirDensity(Grid.PositionComp.GetPosition());
            return new SVariableFloat(val ?? 0);
        }

        private SVariable Altitude(SVariable[] arr)
        {
            var planet = MyGamePruningStructure.GetClosestPlanet(Grid.GetPosition());
            if (planet != null) {
                double altitudeRatio = (Vector3D.Distance(Grid.GetPosition(), planet.PositionComp.GetPosition()) - planet.AverageRadius) / ((double)planet.AtmosphereRadius - planet.AverageRadius);
                return new SVariableFloat((float)altitudeRatio);
            }
            return new SVariableFloat(0);
        }

        private SVariable GroundAltitude(SVariable[] arr)
        {
            var planet = MyGamePruningStructure.GetClosestPlanet(Grid.GetPosition());
            if (planet != null)
            {
                float GroundAltitude = Vector3.Distance(Grid.GetPosition(), planet.GetClosestSurfacePointGlobal(Grid.GetPosition()));
                return new SVariableFloat(GroundAltitude);
            }
            return new SVariableFloat(0);
        }

        private SVariable Speed(SVariable[] arr)
        {
            return new SVariableVector(Grid.LinearVelocity);
        }

        private SVariable IsNPC(SVariable[] arr)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(Grid.BigOwners.FirstOrDefault());
            if (faction != null)
            {
                return new SVariableBool(faction.IsEveryoneNpc());
            }
            return new SVariableBool(false);
        }

    }
}
