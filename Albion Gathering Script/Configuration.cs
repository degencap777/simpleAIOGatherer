using Ennui.Api;
using Ennui.Api.Builder;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public string CityClusterName = "";
        public string ResourceClusterName = "";

        public SafeMapArea GatherArea;
        public SafeMapArea VaultArea;
        public SafeMapArea RepairArea;
        public SafeVector3 VaultDest;
        public SafeVector3 RepairDest;

        public string LoginCharacterName = "";
        public int GatherAttemptsTimeout = 6;

        public bool AttackMobs = false;
        public bool IgnoreMobsOnLowHealth = true;
        public int IgnoreMobHealthPercent = 60;

        public bool FleeOnLowHealth = true;
        public int FleeHealthPercent = 30;

        public float MaxHoldWeight = 100.0f;

        public List<SafeTypeSet> TypeSetsToUse = new List<SafeTypeSet>();
        public List<SafeVector3> RoamPoints = new List<SafeVector3>();
    }
}
