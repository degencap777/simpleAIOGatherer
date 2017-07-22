using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class RepairState : StateScript
    {
        private Configuration config;
        private Context context;

        public RepairState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                context.State = "Failed to find local player!";
                return 10_000;
            }

            if (!config.RepairArea.Contains(localPlayer.ThreadSafeLocation))
            {
                context.State = "Walking to repair area...";

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
                localPlayer.ToggleMount(false);
            }

            if (!Api.HasBrokenItems())
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

            if (!RepairWindow.IsOpen)
            {
                context.State = "Opening repair building...";

                var building = Objects.RepairChain.Closest(localPlayer.ThreadSafeLocation);
                if (building == null)
                {
                    context.State = "Failed to find repair building!";
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
                context.State = "Repairing...";

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
