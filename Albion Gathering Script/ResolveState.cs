using Ennui.Api;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class ResolveState : StateScript
    {
        private Configuration config;

        public ResolveState(Configuration config)
        {
            this.config = config;
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

            var localPlayer = Players.LocalPlayer;
            if (localPlayer != null)
            {
                if (Inventory.HasBrokenItems(10))
                {
                    parent.EnterState("repair");
                }
                else if (localPlayer.TotalHoldWeight >= 99)
                {
                    parent.EnterState("bank");
                }
                else
                {
                    parent.EnterState("gather");
                }
                return 0;
            }

            Logging.Log("Failed to resolve state, trying again later...", LogLevel.Warning);
            return 10_000;
        }
    }
}
