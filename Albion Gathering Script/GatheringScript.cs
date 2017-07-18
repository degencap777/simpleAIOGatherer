using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    [LocalScript]
    public class GatheringState : StateScript
    {
        private Configuration config;
        private Timer timer;

        public override bool OnStart(IScriptEngine se)
        {
            config = new Configuration();
            timer = new Timer();

            AddState("config", new ConfigState(config));
            AddState("resolve", new ResolveState(config));
            AddState("gather", new GatherState(config));
            AddState("combat", new CombatState(config));
            AddState("repair", new RepairState(config));
            AddState("bank", new BankState(config));
            EnterState("config");
            return base.OnStart(se);
        }

        public override void OnPaint(IScriptEngine se, GraphicContext g)
        {
            g.SetColor(new Color(0.3f, 0.3f, 0.3f, 1.0f));
            g.FillRect(15, 100, 265, 150);
            g.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            g.DrawString("http://ennui.ninja - Simple AIO Gatherer", 20, 100);
            g.DrawString(string.Format("Runtime: {0}", Time.FormatElapsed(timer.ElapsedMs)), 20, 115);
            g.DrawString(string.Format("State: {0}", RunningKey), 20, 130);

            if (RunningKey != null && RunningKey != "config")
            {
                var lp = Players.LocalPlayer;
                if (lp != null)
                {
                    var harvestables = Objects
                        .HarvestableChain
                        .FilterDepleted()
                        .FilterByArea(lp.ThreadSafeLocation.Expand(50, 100, 50))
                        .FilterByTypeSet(config.TypeSetsToUse.ToArray());

                    var harvestableCount = 0;
                    foreach (var harvestable in harvestables.AsList)
                    {
                        var bounds = harvestable.InteractBounds;
                        if (bounds != null)
                        {
                            bounds.Render(Api, Color.SkyBlue, Color.SkyBlue.MoreTransparent());
                        }

                        harvestableCount += 1;
                        if (harvestableCount >= 20)
                        {
                            break;
                        }
                    }

                    foreach (var mob in Entities
                        .MobChain
                        .FilterByArea(lp.ThreadSafeLocation.Expand(50, 100, 50))
                        .AsList)
                    {
                        var drops = mob
                            .HarvestableDropChain
                            .FilterByTypeSet(config.TypeSetsToUse.ToArray())
                            .AsList;

                        if (drops.Count > 0)
                        {
                            mob.ThreadSafeLocation.Expand(3, 3, 3).Render(Api, Color.SkyBlue, Color.SkyBlue.MoreTransparent());
                        }
                    }
                }
            }

            foreach (var p in config.RoamPoints)
            {
                p.Expand(3, 3, 3).Render(Api, Color.Red, Color.Red.MoreTransparent());
            }

            if (config.VaultArea != null)
            {
                config.VaultArea.Render(Api, Color.Cyan, Color.Cyan.MoreTransparent());
            }

            if (config.RepairArea != null)
            {
                config.RepairArea.Render(Api, Color.Purple, Color.Purple.MoreTransparent());
            }
        }
    }
}
