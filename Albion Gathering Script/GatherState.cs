using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class GatherState : StateScript
    {
        private Configuration config;
        private IHarvestableObject harvestableTarget;
        private IMobObject mobTarget;
        private int gatherAttempts = 0;

        public GatherState(Configuration config)
        {
            this.config = config;
        }

        private void Reset()
        {
            harvestableTarget = null;
            mobTarget = null;
            gatherAttempts = 0;
        }

        private bool NeedsNew()
        {
            if (harvestableTarget == null && mobTarget == null)
            {
                return true;
            }

            if (mobTarget != null && (!mobTarget.IsValid || mobTarget.CurrentHealth <= 0))
            {
                return true;
            }

            if (harvestableTarget != null && (!harvestableTarget.IsValid || harvestableTarget.Depleted))
            {
                return true;
            }

            if (gatherAttempts >= config.GatherAttemptsTimeout)
            {
                return true;
            }

            return false;
        }

        private void FindResource(Vector3<float> center)
        {
            Reset();
            var territoryAreas = new List<MapArea>();
        }

        public override int OnLoop(IScriptEngine se)
        {
            if (Api.HasBrokenItems())
            {
                parent.EnterState("repair");
                return 0;
            }

            var local = Players.LocalPlayer;
            if (local == null)
            {
                Logging.Log("Failed to find local player!", LogLevel.Error);
                return 10_000;
            }

            if (local.IsUnderAttack)
            {
                Logging.Log("Local player under attack, fight back!", LogLevel.Atom);
                parent.EnterState("combat");
                return 0;
            }

            var localLocation = local.ThreadSafeLocation;
            if (!config.GatherArea.Contains(localLocation))
            {
                Logging.Log("Local player not in gather area, walk there!", LogLevel.Atom);

                var moveConfig = new PointPathFindConfig();
                moveConfig.UseWeb = false;
                moveConfig.ClusterName = config.ResourceClusterName;
                if (Movement.PathFindTo(moveConfig) != PathFindResult.Success)
                {
                    Logging.Log("Local player failed to find path to resource area!", LogLevel.Error);
                    return 10_000;
                }
                
                return 0;
            }
            
            var weightThreshold = 98;
            if (Equipment.HasItemContainingName("_MOUNT_HORSE"))
            {
                weightThreshold = 130;
            }
            else if (Equipment.HasItemContainingName("_MOUNT_OX"))
            {
                weightThreshold = 450;
            }

            Logging.Log("Weight threshold: " + weightThreshold, LogLevel.Atom);
            if (local.WeighedDownPercent >= weightThreshold)
            {
                Logging.Log("Local player has too much weight, banking!", LogLevel.Atom);
                parent.EnterState("bank");
                return 0;
            }

            if (NeedsNew())
            {

            }

            return 100;
        }
    }
}
