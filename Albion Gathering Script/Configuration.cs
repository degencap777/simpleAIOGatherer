using Ennui.Api;
using Ennui.Api.Builder;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public string CityClusterName { get; set; }
        public string ResourceClusterName { get; set; }

        public IArea<float> GatherArea { get; set; }
        public IArea<float> VaultArea { get; set; }
        public Vector3<float> VaultDest { get; set; }
        public IArea<float> RepairArea { get; set; }
        public Vector3<float> RepairDest { get; set; }

        public string LoginCharacterName { get; set; } = "";
        public int GatherAttemptsTimeout { get; set; } = 6;

        public bool AttackMobs { get; set; } = false;
        public bool IgnoreMobsOnLowHealth { get; set; } = true;
        public int IgnoreMobHealthPercent { get; set; } = 60;

        public bool FleeOnLowHealth { get; set; } = true;
        public int FleeHealthPercent { get; set; } = 30;

        public List<TypeSet> TypeSetsToUse { get; set; } = new List<TypeSet>();
    }
}
