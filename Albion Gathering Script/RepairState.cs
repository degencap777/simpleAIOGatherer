using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class RepairState : StateScript
    {
        private Configuration config;

        public RepairState(Configuration config)
        {
            this.config = config;
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            if (localPlayer != null)
            {
                if (!config.RepairArea.Contains(localPlayer.ThreadSafeLocation))
                {
                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.CityClusterName;
                    config.UseWeb = false;
                    config.Point = this.config.RepairDest;
                    Movement.PathFindTo(config);
                    return 0;
                }

                if (localPlayer.IsMounted)
                {
                    localPlayer.ToggleMount();
                }

                if (!Inventory.HasBrokenItems(70))
                {
                    if (localPlayer.WeighedDownPercent >= 90)
                    {
                        parent.EnterState("bank");
                    }
                    else
                    {
                        parent.EnterState("gather");
                    }
                }
                return 0;
            }

            if (!RepairWindow.IsOpen)
            {
                var building = Objects.RepairChain.Closest(localPlayer.ThreadSafeLocation);
                if (building == null)
                {
                    Logging.Log("failed to find repair building");
                    return 5000;
                }

                building.Click();
                Time.SleepUntil(() =>
                  {
                      return RepairWindow.IsOpen;
                  }, 4000);
            }

            if (RepairWindow.IsOpen)
            {
                if (RepairWindow.RepairAll())
                {
                    Time.SleepUntil(() =>
                    {
                        return !Inventory.HasBrokenItems(70);
                    }, 60000);
                }
            }

            return 100;
        }
    }
}
