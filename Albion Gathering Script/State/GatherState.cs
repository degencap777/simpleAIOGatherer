using Ennui.Api.Meta;
using Ennui.Api.Method;
using Ennui.Api.Object;
using Ennui.Api.Script;
using Ennui.Api.Util;
using System;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class GatherState : StateScript
    {
        private Configuration config;
        private Context context;

        private IHarvestableObject harvestableTarget;
        private IMobObject mobTarget;
        private int gatherAttempts = 0;
        private List<long> blacklist = new List<long>();
        private Random rand = new Random();

        public GatherState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
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
                if (harvestableTarget != null)
                {
                    blacklist.Add(harvestableTarget.Id);
                }
                if (mobTarget != null)
                {
                    blacklist.Add(mobTarget.Id);
                }
                return true;
            }

            return false;
        }

        private IMobObject Closest(Vector3<float> center, List<IMobObject> mobs)
        {
            var dist = 0.0f;
            IMobObject closest = null;
            foreach (var m in mobs)
            {
                var cdist = m.ThreadSafeLocation.SimpleDistance(center);
                if (closest == null || cdist < dist)
                {
                    dist = cdist;
                    closest = m;
                }
            }

            return closest;
        }

        private bool FindResource(Vector3<float> center)
        {
            context.State = "Finding resource...";

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

            if (config.AttackMobs)
            {
                var lpo = Players.LocalPlayer;
                if (lpo != null && config.IgnoreMobsOnLowHealth && lpo.HealthPercentage > config.IgnoreMobHealthPercent)
                {
                    var resourceMobs = new List<IMobObject>();
                    foreach (var ent in Entities.MobChain.ExcludeByArea(territoryAreas.ToArray()).ExcludeWithIds(blacklist.ToArray()).AsList)
                    {
                        var drops = ent.HarvestableDropChain.FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse)).AsList;
                        Logging.Log(drops.ToString());
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

            harvestableTarget = Objects
                .HarvestableChain
                .FilterDepleted()
                .ExcludeWithIds(blacklist.ToArray())
                .ExcludeByArea(territoryAreas.ToArray())
                .FilterByTypeSet(SafeTypeSet.BatchConvert(config.TypeSetsToUse))
                .FilterWithSetupState(HarvestableSetupState.Invalid)
                .FilterWithSetupState(HarvestableSetupState.Owned)
                .Closest(center);

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

        private Vector3<float> RandomRoamPoint()
        {
            if (config.RoamPoints.Count == 0)
            {
                return null;
            }
            if (config.RoamPoints.Count == 1)
            {
                return config.RoamPoints[0].RealVector3();
            }
            return config.RoamPoints[rand.Next(config.RoamPoints.Count)].RealVector3();
        }

        private Boolean ShouldUseMount(float heldWeight, float dist)
        {
            var useMount = false;

            if (dist >= 15)
            {
                useMount = true;
            }
            else if (heldWeight >= 100 && dist >= 6)
            {
                useMount = true;
            }
            else if (heldWeight >= 120 && dist >= 3)
            {
                useMount = true;
            }
            else if (heldWeight >= 135)
            {
                useMount = true;
            }

            return useMount;
        }

        public override int OnLoop(IScriptEngine se)
        {
            if (config.RepairDest != null && Api.HasBrokenItems())
            {
                parent.EnterState("repair");
                return 0;
            }

            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                context.State = "Failed to find local player!";
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
            if (!config.ResourceArea.RealArea(Api).Contains(localLocation))
            {
                context.State = "Walking to gather area...";
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

            var heldWeight = localPlayer.TotalHoldWeight;
            if (heldWeight >= config.MaxHoldWeight)
            {
                Logging.Log("Local player has too much weight, banking!", LogLevel.Atom);
                parent.EnterState("bank");
                return 0;
            }

            if (NeedsNew())
            {
                if (!FindResource(localLocation))
                {
                    var point = RandomRoamPoint();
                    if (point == null)
                    {
                        context.State = "Failed to find roam point!";
                        Logging.Log("Cannot roam as roam points were not added!");
                        return 15000;
                    }

                    context.State = "Roaming";
                    var moveConfig = new PointPathFindConfig();
                    moveConfig.UseWeb = false;
                    moveConfig.ClusterName = config.ResourceClusterName;
                    moveConfig.Point = point;
                    moveConfig.UseMount = true;
                    moveConfig.ExitHook = (() =>
                    {
                        var local = Players.LocalPlayer;
                        if (local != null)
                        {
                            return FindResource(local.ThreadSafeLocation);
                        }
                        return false;
                    });

                    if (Movement.PathFindTo(moveConfig) != PathFindResult.Success)
                    {
                        context.State = "Failed to find path to roaming point...";
                        return 10_000;
                    }
                    return 5000;
                }
            }

            if (harvestableTarget != null)
            {
                var area = harvestableTarget.InteractBounds;
                if (area.Contains(localLocation))
                {
                    if (localPlayer.IsMounted)
                    {
                        localPlayer.ToggleMount(false);
                    }

                    context.State = "Attempting to gather " + harvestableTarget.Id + " (" + gatherAttempts + ")";
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
                    context.State = "Walking to resource...";

                    var dist = localLocation.SimpleDistance(harvestableTarget.ThreadSafeLocation);
                    var config = new ResourcePathFindConfig();
                    config.ClusterName = this.config.ResourceClusterName;
                    config.UseWeb = false;
                    config.Target = harvestableTarget;
                    config.UseMount = ShouldUseMount(heldWeight, dist);
                    config.ExitHook = (() =>
                    {
                        var lpo = Players.LocalPlayer;
                        if (lpo == null) return false;

                        if (!lpo.IsMounted && lpo.IsUnderAttack)
                        {
                            parent.EnterState("combat");
                            return true;
                        }

                        if (!harvestableTarget.IsValid || harvestableTarget.Depleted)
                        {
                            return true;
                        }

                        return false;
                    });

                    var result = Movement.PathFindTo(config);
                    if (result == PathFindResult.Failed)
                    {
                        context.State = "Failed to path find to resource!";
                        blacklist.Add(harvestableTarget.Id);
                        Reset();
                    }
                    return 0;
                }
            }
            else if (mobTarget != null)
            {
                var mobGatherArea = mobTarget.ThreadSafeLocation.Expand(3, 3, 3);
                if (mobGatherArea.Contains(localLocation))
                {
                    context.State = "Attempting to kill mob";

                    if (localPlayer.IsMounted)
                    {
                        localPlayer.ToggleMount(false);
                    }

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
                    context.State = "Walking to mob...";

                    var dist = localLocation.SimpleDistance(mobTarget.ThreadSafeLocation);
                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.ResourceClusterName;
                    config.UseWeb = false;
                    config.Point = mobTarget.ThreadSafeLocation;
                    config.UseMount = ShouldUseMount(heldWeight, dist);
                    config.ExitHook = (() =>
                    {
                        var lpo = Players.LocalPlayer;
                        if (lpo == null) return false;

                        if (!lpo.IsMounted && lpo.IsUnderAttack)
                        {
                            parent.EnterState("combat");
                            return true;
                        }

                        if (!mobTarget.IsValid || mobTarget.CurrentHealth <= 0)
                        {
                            return true;
                        }

                        return false;
                    });

                    if (Movement.PathFindTo(config) == PathFindResult.Failed)
                    {
                        context.State = "Failed to path find to mob!";
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
