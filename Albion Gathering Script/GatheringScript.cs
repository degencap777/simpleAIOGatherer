using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    [LocalScript]
    public class GatheringState : StateScript
    {
        private Configuration config;
        private Context context;
        private Timer timer;

        private void LoadConfig()
        {
            try
            {
                if (Files.Exists("simple-aio-gatherer.json"))
                {
                    config = Codecs.FromJson<Configuration>(Files.ReadAllText("simple-aio-gatherer.json"));
                    if (config.TypeSetsToUse == null)
                    {
                        config.TypeSetsToUse = new List<SafeTypeSet>();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log("Failed to log config " + e, LogLevel.Error);
            }

            if (config == null)
            {
                config = new Configuration();
            }
        }
        public override bool OnStart(IScriptEngine se)
        {
            LoadConfig();

            context = new Context();
            timer = new Timer();

            AddHook(() =>
            {
                if (base.RunningKey != "login" && config.AutoRelogin && (LoginWindow.IsOpen || CharacterSelectWindow.IsOpen || LoginErrorWindow.IsOpen))
                {
                    EnterState("login");
                }
                return false;
            });

            AddState("config", new ConfigState(config, context));
            AddState("resolve", new ResolveState(config, context));
            AddState("gather", new GatherState(config, context));
            AddState("combat", new CombatState(config, context));
            AddState("repair", new RepairState(config, context));
            AddState("bank", new BankState(config, context));
            AddState("login", new LoginState(config, context));
            EnterState("config");
            return base.OnStart(se);
        }

        public override void OnPaint(IScriptEngine se, GraphicContext g)
        {
            g.SetColor(new Color(0.3f, 0.3f, 0.3f, 1.0f));
            g.FillRect(15, 100, 265, 195);
            g.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            g.DrawString("http://ennui.ninja - Simple AIO Gatherer", 20, 100);
            g.DrawString(string.Format("Runtime: {0}", Time.FormatElapsed(timer.ElapsedMs)), 20, 115);
            g.DrawString(string.Format("State: {0}", context.State), 20, 130);
            g.DrawString(string.Format("Max hold weight: {0}", config.MaxHoldWeight), 20, 145);
            g.DrawString(string.Format("City cluster: {0}", config.CityClusterName), 20, 160);
            g.DrawString(string.Format("Resource cluster: {0}", config.ResourceClusterName), 20, 175);

            if (config.ResourceClusterName == Game.ClusterName)
            {
                foreach (var p in config.RoamPoints)
                {
                    p.RealVector3().Expand(3, 3, 3).Render(Api, Color.Red, Color.Red.MoreTransparent());
                }
            }

            if (config.VaultArea != null && config.CityClusterName == Game.ClusterName)
            {
                config.VaultArea.RealArea(Api).Render(Api, Color.Cyan, Color.Cyan.MoreTransparent());
            }

            if (config.RepairArea != null && config.CityClusterName == Game.ClusterName)
            {
                config.RepairArea.RealArea(Api).Render(Api, Color.Purple, Color.Purple.MoreTransparent());
            }
        }
    }
}
