﻿using Ennui.Api.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return 100;
        }
    }
}
