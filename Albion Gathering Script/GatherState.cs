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
        private List<long> blacklist = new List<long>();

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

        private IMobObject Closest(Vector3<float> center, List<IMobObject> mobs)
        {
            var dist = 2147000000.0f;
            IMobObject closest = null;
            foreach (var m in mobs)
            {
                var cdist = m.ThreadSafeLocation.SimpleDistance(center);
                if (cdist < dist)
                {
                    dist = cdist;
                    closest = m;
                }
            }

            return closest;
        }

        private bool FindResource(Vector3<float> center)
        {
            Reset();
            var territoryAreas = new List<Area>();
            var graph = Graphs.LookupByDisplayName(Game.ClusterName);
            foreach (var t in graph.Territories)
            {
                var tCenter = t.Center;
                var tSize = t.Size;
                var tCenter3d = new Vector3f(tCenter.X, 0, tCenter.Y);
                var tBegin = tCenter3d.Translate(0 - (tSize.X / 2), -100, 0 - (tSize.Y / 2));
                var tEnd = tCenter3d.Translate(tSize.X / 2, 100, tSize.Y / 2);
                var tArea = new Area(tBegin, tEnd);
                territoryAreas.Add(tArea);
            }

            //find mob target
            if (config.AttackMobs)
            {
                var lpo = Players.LocalPlayer;
                if (lpo != null && config.IgnoreMobsOnLowHealth && lpo.HealthPercentage > config.IgnoreMobHealthPercent)
                {
                    var resourceMobs = new List<IMobObject>();
                    foreach (var ent in Entities.MobChain.ExcludeByArea(territoryAreas.ToArray()).AsList)
                    {
                        var drops = ent.HarvestableDropChain.FilterByTypeSet(config.TypeSetsToUse.ToArray()).AsList;
                        if (drops.Count > 0)
                        {
                            resourceMobs.Add(ent);
                        }
                    }

                    if (resourceMobs.Count > 0)
                    {
                        mobTarget = Closest(center, resourceMobs);
                    }
                }
            }

            // find resource target
            harvestableTarget = Objects
                .HarvestableChain
                .FilterDepleted()
                .ExcludeWithIds(blacklist.ToArray())
                .ExcludeByArea(territoryAreas.ToArray())
                .FilterByTypeSet(config.TypeSetsToUse.ToArray())
                .Closest(center);

            //if we found both types of target, decide which one to use
            if (mobTarget != null && harvestableTarget != null)
            {
                var mobDist = mobTarget.ThreadSafeLocation.SimpleDistance(center);
                var resDist = harvestableTarget.ThreadSafeLocation.SimpleDistance(center);
                if (mobDist < resDist)
                {
                    harvestableTarget = null;
                }
                else
                {
                    mobTarget = null;
                }
            }

            return harvestableTarget != null || mobTarget != null;
        }

        public override int OnLoop(IScriptEngine se)
        {
            if (Api.HasBrokenItems())
            {
                parent.EnterState("repair");
                return 0;
            }

            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                Logging.Log("Failed to find local player!", LogLevel.Error);
                return 10_000;
            }

            if (localPlayer.IsUnderAttack)
            {
                Logging.Log("Local player under attack, fight back!", LogLevel.Atom);
                parent.EnterState("combat");
                return 0;
            }

            var localLocation = localPlayer.ThreadSafeLocation;
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
            if (localPlayer.WeighedDownPercent >= weightThreshold)
            {
                Logging.Log("Local player has too much weight, banking!", LogLevel.Atom);
                parent.EnterState("bank");
                return 0;
            }

            if (NeedsNew())
            {
                if (!FindResource(localLocation))
                {
                    Logging.Log("failed to find resource");
                    Movement.PathRandomly(config.ResourceClusterName, () =>
                    {
                        var localLoc = Players.LocalLocation;
                        if (localLoc == null)
                        {
                            return false;
                        }


                        return FindResource(localLoc);
                    });
                    return 5000;
                }
            }

            if (harvestableTarget != null)
            {
                Logging.Log("Gather resource begin");
                
                var area = harvestableTarget.InteractBounds;
                if (area.Contains(localLocation))
                {
                    Logging.Log("in gather area");


                    if (localPlayer.IsMounted)
                    {
                        localPlayer.ToggleMount(false);
                    }


                    Logging.Log(string.Format("attempting to gather {0}", gatherAttempts));
                    harvestableTarget.Click();


                    Time.SleepUntil(() =>
                    {
                        return localPlayer.IsHarvesting;
                    }, 3000);

                    if (!localPlayer.IsHarvesting)
                    {
                        gatherAttempts = gatherAttempts + 1;
                    }


                    return 100;
                }
                else
                {
                    var config = new ResourcePathFindConfig();
                    config.ClusterName = this.config.ResourceClusterName;
                    config.UseWeb = false;
                    config.Target = harvestableTarget;
                    
                    var result = Movement.PathFindTo(config);
                    if (result == PathFindResult.Failed)
                    {
                        blacklist.Add(harvestableTarget.Id);
                        Reset();
                    }
                    return 0;
                }
            }
            else if (mobTarget != null)
            {
                Logging.Log("Gather mob begin");


                var mobGatherArea = mobTarget.ThreadSafeLocation.Expand(3, 3, 3);
                if (mobGatherArea.Contains(localLocation))
                {
                    if (localPlayer.IsMounted)
                    {
                        localPlayer.ToggleMount(false);
                    }

                    Logging.Log(string.Format("attempting to attack mob {0}", gatherAttempts));
                    localPlayer.SetSelectedObject(mobTarget);
                    localPlayer.AttackSelectedObject();

                    Time.SleepUntil(() =>
                    {
                        return localPlayer.IsUnderAttack;
                    }, 3000);

                    if (localPlayer.IsUnderAttack)
                    {
                        parent.EnterState("combat");
                    }
                    else
                    {
                        gatherAttempts = gatherAttempts + 1;
                    }


                    return 100;
                }
                else
                {
                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.ResourceClusterName;
                    config.UseWeb = false;
                    config.Point = mobTarget.ThreadSafeLocation;


                    if (Movement.PathFindTo(config) == PathFindResult.Failed)
                    {
                        blacklist.Add(mobTarget.Id);
                        Reset();
                    }
                    return 0;
                }
            }
            return 100;
        }
    }
}
