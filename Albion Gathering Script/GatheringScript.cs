using Ennui.Api;
using Ennui.Api.Method;
using Ennui.Api.Script;

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
            g.FillRect(15, 100, 265, 145);
            g.SetColor(new Color(0.95f, 0.5f, 0.0f, 1.0f));
            g.DrawString("http://ennui.ninja - Simple AIO Gatherer", 20, 100);
            g.DrawString(string.Format("Runtime: {0}", Time.FormatElapsed(timer.ElapsedMs)), 20, 115);
            g.DrawString(string.Format("State: {0}", RunningKey), 20, 130);
        }
    }
}
