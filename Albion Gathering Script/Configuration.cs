using Ennui.Api;
using Ennui.Api.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public string CityClusterName { get; set; }
        public string ResourceClusterName { get; set; }

        public IArea<float> GatherArea { get; set; }
        public IArea<float> VaultArea { get; set; }
        public IArea<float> RepairArea { get; set; }

        public string LoginCharacterName { get; set; } = "";
        public int GatherAttemptsTimeout { get; set; } = 6;
        public bool AttackMobs { get; set; } = false;

        public List<TypeSet> TypeSetsToUse { get; set; } = new List<TypeSet>();
    }
}
