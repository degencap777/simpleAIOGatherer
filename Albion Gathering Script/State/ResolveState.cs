using Ennui.Api.Script;
using Ennui.Api.Util;

namespace Ennui.Script.Official
{
    public class ResolveState : StateScript
    {
        private Configuration config;
        private Context context;

        public ResolveState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
            Time.SleepUntil(() => !Game.InLoadingScreen, 30000);
            if (Game.InLoadingScreen)
            {
                Logging.Log("In loading screen too long, exiting script...", LogLevel.Error);
                se.StopScript();
                return 0;
            }

            context.State = "Resolving...";

            var localPlayer = Players.LocalPlayer;
            if (localPlayer != null)
            {
                if (config.RepairDest != null && Api.HasBrokenItems())
                {
                    parent.EnterState("repair");
                }
                else if (localPlayer.TotalHoldWeight >= config.MaxHoldWeight)
                {
                    parent.EnterState("bank");
                }
                else
                {
                    parent.EnterState("gather");
                }
                return 0;
            }

            return 10_000;
        }
    }
}
