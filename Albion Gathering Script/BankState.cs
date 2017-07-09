using Ennui.Api;
using Ennui.Api.Direct;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ennui.Script.Official
{
    public class BankState : StateScript
    {
        private Configuration config;

        public BankState(Configuration config)
        {
            this.config = config;
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            if (localPlayer != null)
            {
                if (!config.VaultArea.Contains(localPlayer.ThreadSafeLocation))
                {
                    var config = new PointPathFindConfig();
                    config.ClusterName = this.config.CityClusterName;
                    config.UseWeb = false;
                    config.Point = this.config.VaultDest;


                    Movement.PathFindTo(config);
                    return 0;
                }


                if (config.FleeOnLowHealth && localPlayer.HealthPercentage <= config.FleeHealthPercent)
                {
                    return 1000;
                }


                if (!Banking.IsOpen)
                {
                    var bank = Objects.BankChain.Closest(localPlayer.ThreadSafeLocation);
                    if (bank == null)
                    {
                        Logging.Log("failed to find bank vault building");
                        return 5000;
                    }

                    bank.Click();
                    Time.SleepUntil(() =>
                    {
                        return Banking.IsOpen;
                    }, 4000);
                }


                if (Banking.IsOpen)
                {
                    var beginWeight = localPlayer.WeighedDownPercent;


                    var allItems = Inventory.GetItemsBySubstring("_ROCK", "_ORE", "_HIDE", "_WOOD", "_FIBER");
                    var toDeposit = new List<IItemStack>();
                    foreach (var stack in allItems)
                    {
                        if (stack.UniqueName.Contains("JOURNAL"))
                        {
                            toDeposit.Add(stack);
                        }
                    }
                    
                    if (toDeposit.Count == 0 || Banking.Deposit(toDeposit.ToArray()))
                    {

                        if (localPlayer.TotalHoldWeight < 99)
                        {
                            if (Inventory.HasBrokenItems(70))
                            {
                                parent.EnterState("repair");
                            }
                            else
                            {
                                parent.EnterState("gather");
                            }
                            return 0;
                        }
                    }
                }
            }

            return 100;
        }
    }
}
