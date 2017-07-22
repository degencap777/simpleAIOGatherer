﻿using Ennui.Api;
using Ennui.Api.Direct.Object;
using Ennui.Api.Meta;
using Ennui.Api.Script;

namespace Ennui.Script.Official
{
    public class CombatState : StateScript
    {
        private Configuration config;
        private Context context;

        public CombatState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        private void HandleSpellRotation(ILocalPlayerObject self, IEntityObject target)
        {
            if (self.CurrentActionState == ActionState.Casting)
            {
                return;
            }

            context.State = "Casting spell!";

            var buffSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Buff).First;
            if (buffSelfSpell != null)
            {
                self.CastOnSelf(buffSelfSpell.Slot);
            }

            var instantSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.Instant).ExcludeWithName("ESCAPE_DUNGEON").First;
            if (instantSelfSpell != null)
            {
                self.CastOnSelf(instantSelfSpell.Slot);
            }

            var movBufSelfSpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Self).FilterByCategory(SpellCategory.MovementBuff).First;
            if (movBufSelfSpell != null)
            {
                self.CastOnSelf(movBufSelfSpell.Slot);
            }

            var buffEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Buff).First;
            if (buffEnemySpell != null)
            {
                self.CastOn(buffEnemySpell.Slot, target);
            }

            var debuffEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Debuff).First;
            if (debuffEnemySpell != null)
            {
                self.CastOn(debuffEnemySpell.Slot, target);
            }

            var dmgEnemySpell = self.SpellChain.FilterByReady().FilterByTarget(SpellTarget.Enemy).FilterByCategory(SpellCategory.Damage).First;
            if (dmgEnemySpell != null)
            {
                self.CastOn(dmgEnemySpell.Slot, target);
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

            return 100;
        }
    }
}