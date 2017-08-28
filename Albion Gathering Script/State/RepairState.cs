using Ennui.Api.Method;
using Ennui.Api.Script;
using Ennui.Api.Util;

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

            if (!config.RepairArea.RealArea(Api).Contains(localPlayer.ThreadSafeLocation))
            {
                context.State = "Walking to repair area...";

                var config = new PointPathFindConfig();
                config.ClusterName = this.config.RepairClusterName;
                config.Point = this.config.RepairDest.RealVector3();
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
                if (localPlayer.WeighedDownPercent >= 30)
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
