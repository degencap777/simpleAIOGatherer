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
            Logging.Log("Attempting to repair...");

            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                Logging.Log("Failed to find local player!", LogLevel.Warning);
                return 10_000;
            }

            if (!config.RepairArea.Contains(localPlayer.ThreadSafeLocation))
            {
                Logging.Log("Moving to repair area");

                var config = new PointPathFindConfig();
                config.ClusterName = this.config.CityClusterName;
                config.Point = this.config.RepairDest;
                config.UseWeb = false;
                config.UseMount = true;
                Movement.PathFindTo(config);
                return 0;
            }

            if (localPlayer.IsMounted)
            {
                Logging.Log("Getting off mount...");
                localPlayer.ToggleMount();
            }

            if (!Api.HasBrokenItems())
            {
                if (localPlayer.WeighedDownPercent >= 90)
                {
                    Logging.Log("No broke items, leaving to bank");
                    parent.EnterState("bank");
                }
                else
                {
                    Logging.Log("No broke items, leaving to gather");
                    parent.EnterState("gather");
                }
            }

            if (!RepairWindow.IsOpen)
            {
                Logging.Log("Attempting to find repair building...");
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
                Logging.Log("Repair window is open!");
                if (RepairWindow.RepairAll())
                {
                    Time.SleepUntil(() =>
                    {
                        return !Api.HasBrokenItems();
                    }, 60000);
                }
            }

            return 100;
        }
    }
}
