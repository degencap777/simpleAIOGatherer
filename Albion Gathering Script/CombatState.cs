using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class CombatState : StateScript
    {
        private Configuration config;

        public CombatState(Configuration config)
        {
            this.config = config;
        }

        public override int OnLoop(IScriptEngine se)
        {
            // TODO
            return 100;
        }
    }
}
