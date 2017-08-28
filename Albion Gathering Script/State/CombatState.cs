using Ennui.Api;
using Ennui.Api.Meta;
using Ennui.Api.Object;
using Ennui.Api.Script;
using Ennui.Api.Util;

namespace Ennui.Script.Official
{
    public class CombatState : StateScript
    {
        private Configuration config;
        private Context context;
        private StateMonitor<ActionState> actionStateMonitor;

        public CombatState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        private void HandleSpellRotation(ILocalPlayerObject self, IEntityObject target)
        {
            if (!actionStateMonitor.Stamp(self.CurrentActionState))
            {
                context.State = "Waiting for spell cast";
                return;
            }

            context.State = "Casting spell!";

            var buffSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Buff).First;
            if (buffSelfSpell != null)
            {
                self.CastOnSelf(buffSelfSpell.Slot);
                return;
            }

            var instantSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Instant).ExcludeWithName("ESCAPE_DUNGEON").First;
            if (instantSelfSpell != null)
            {
                self.CastOnSelf(instantSelfSpell.Slot);
                return;
            }

            var movBufSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.MovementBuff).First;
            if (movBufSelfSpell != null)
            {
                self.CastOnSelf(movBufSelfSpell.Slot);
                return;
            }

            var buffEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Buff).First;
            if (buffEnemySpell != null)
            {
                self.CastOn(buffEnemySpell.Slot, target);
                return;
            }

            var debuffEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Debuff).First;
            if (debuffEnemySpell != null)
            {
                self.CastOn(debuffEnemySpell.Slot, target);
                return;
            }

            var dmgEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Damage).First;
            if (dmgEnemySpell != null)
            {
                self.CastOn(dmgEnemySpell.Slot, target);
                return;
            }

            var dmgSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Damage).First;
            if (dmgSelfSpell != null)
            {
                self.CastOnSelf(dmgSelfSpell.Slot);
                return;
            }

            var dmgGroundSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Ground).FilterByCategory(SpellCategory.Damage).First;
            if (dmgGroundSpell != null)
            {
                self.CastAt(dmgGroundSpell.Slot, target.ThreadSafeLocation);
                return;
            }

            var crowdControlGroundSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Ground).FilterByCategory(SpellCategory.CrowdControl).First;
            if (crowdControlGroundSpell != null)
            {
                self.CastAt(crowdControlGroundSpell.Slot, target.ThreadSafeLocation);
                return;
            }

            var crowdControlEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.CrowdControl).First;
            if (crowdControlEnemySpell != null)
            {
                self.CastOn(crowdControlEnemySpell.Slot, target);
                return;
            }

            if (self.HealthPercentage <= 50)
            {
                var healSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Heal).First;
                if (healSelfSpell != null)
                {
                    self.CastOnSelf(healSelfSpell.Slot);
                    return;
                }

                var healAllSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.All).FilterByCategory(SpellCategory.Heal).First;
                if (healAllSpell != null)
                {
                    self.CastOnSelf(healAllSpell.Slot);
                    return;
                }
            }
        }

        public override bool OnStart(IScriptEngine se)
        {
            actionStateMonitor = new StateMonitor<ActionState>(Api, 7, ActionState.Idle, ActionState.Attacking);
            return base.OnStart(se);
        }

        public override int OnLoop(IScriptEngine se)
        {
            var localPlayer = Players.LocalPlayer;
            if (localPlayer == null)
            {
                context.State = "Failed to find local player!";
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
                context.State = "Killing mob!";

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

            return 200;
        }
    }
}
