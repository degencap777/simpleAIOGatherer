using Ennui.Api;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class Configuration
    {
        public SafeMapArea ResourceArea;
        public string ResourceClusterName = "";

        public SafeMapArea VaultArea;
        public SafeVector3 VaultDest;
        public string VaultClusterName = "";
      
        public SafeMapArea RepairArea;
        public SafeVector3 RepairDest;
        public string RepairClusterName = "";

        public bool AutoRelogin = false;
        public string LoginCharacterName = "";
        public int GatherAttemptsTimeout = 6;

        public bool AttackMobs = false;
        public bool IgnoreMobsOnLowHealth = true;
        public int IgnoreMobHealthPercent = 60;

        public bool FleeOnLowHealth = true;
        public int FleeHealthPercent = 30;

        public float MaxHoldWeight = 100.0f;

        public bool GatherWood = false;
        public bool GatherOre = false;
        public bool GatherFiber = false;
        public bool GatherHide = false;
        public bool GatherStone = false;
       
        public List<SafeTypeSet> TypeSetsToUse = new List<SafeTypeSet>();
        public List<SafeVector3> RoamPoints = new List<SafeVector3>();
    }
}
