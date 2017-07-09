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
            return 100;
        }
    }
}
