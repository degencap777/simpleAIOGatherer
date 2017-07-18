using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Meta;
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

        private void HandleSpellRotation(ILocalPlayerObject self, IEntityObject target)
        {
            var buffSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Buff).ExcludeWithName("ESCAPE_DUNGEON").First;
            if (buffSpell != null)
            {
                self.CastOnSelf(buffSpell.Slot);
            }

            var instantSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Instant).First;
            if (instantSpell != null)
            {
                self.CastOnSelf(instantSpell.Slot);
            }

            var movBuffSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.MovementBuff).First;
            if (movBuffSpell != null)
            {
                self.CastOnSelf(movBuffSpell.Slot);
            }

            var dmgSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Damage).First;
            if (dmgSpell != null)
            {
                self.CastOn(dmgSpell.Slot, target);
            }

            var dmgSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Damage).First;
            if (dmgSelfSpell != null)
            {
                self.CastOnSelf(dmgSelfSpell.Slot);
            }

            var crowdControlSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Ground).FilterByCategory(SpellCategory.CrowdControl).First;
            if (crowdControlSpell != null)
            {
                self.CastAt(crowdControlSpell.Slot, target.ThreadSafeLocation);
            }

            if (self.HealthPercentage <= 50)
            {
                var healSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Heal).First;
                if (healSelfSpell != null)
                {
                    self.CastOnSelf(healSelfSpell.Slot);
                }

                var healAllSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.All).FilterByCategory(SpellCategory.Heal).First;
                if (healAllSpell != null)
                {
                    self.CastOnSelf(healAllSpell.Slot);
                }
            }
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                Logging.Log("Failed to find local player!", LogLevel.Warning);
                return 100;
            }

            if (config.FleeOnLowHealth && localPlayer.HealthPercentage <= config.FleeHealthPercent)
            {
                parent.EnterState("bank");
                return 0;
            }

            if (localPlayer.IsMounted)
            {
                localPlayer.ToggleMount(false);
            }

            if (localPlayer.AttackTarget == null)
            {
                var list = localPlayer.UnderAttackBy;
                if (list.Count > 0)
                {
                    localPlayer.SetSelectedObject(list[0]);
                    localPlayer.AttackSelectedObject();
                    Time.SleepUntil(() =>
                    {
                        return localPlayer.AttackTarget != null;
                    }, 5000);
                }
                else
                {
                    parent.EnterState("gather");
                    return 0;
                }
            }

            var targ = localPlayer.AttackTarget;
            if (targ != null)
            {
                HandleSpellRotation(localPlayer, targ);
            }

            return 100;
        }
    }
}
