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
        private Context context;
        private Timer timer;

        public override bool OnStart(IScriptEngine se)
        {
            config = new Configuration();
            context = new Context();

            timer = new Timer();

            AddState("config", new ConfigState(config, context));
            AddState("resolve", new ResolveState(config, context));
            AddState("gather", new GatherState(config, context));
            AddState("combat", new CombatState(config, context));
            AddState("repair", new RepairState(config, context));
            AddState("bank", new BankState(config, context));
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
            g.DrawString(string.Format("State: {0}", context.State), 20, 130);
            
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
