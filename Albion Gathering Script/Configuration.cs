using Ennui.Api;
using Ennui.Api.Builder;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public string CityClusterName { get; set; }
        public string ResourceClusterName { get; set; }

        public Area GatherArea { get; set; }
        public Area VaultArea { get; set; }
        public Vector3f VaultDest { get; set; }
        public Area RepairArea { get; set; }
        public Vector3f RepairDest { get; set; }

        public string LoginCharacterName { get; set; } = "";
        public int GatherAttemptsTimeout { get; set; } = 6;

        public bool AttackMobs { get; set; } = false;
        public bool IgnoreMobsOnLowHealth { get; set; } = true;
        public int IgnoreMobHealthPercent { get; set; } = 60;

        public bool FleeOnLowHealth { get; set; } = true;
        public int FleeHealthPercent { get; set; } = 30;

        public List<TypeSet> TypeSetsToUse { get; set; } = new List<TypeSet>();
        public List<Vector3f> RoamPoints { get; set; } = new List<Vector3f>();
    }
}
