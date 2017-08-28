using Ennui.Api.Meta;
using Ennui.Api.Util;
using System.Collections.Generic;

namespace Ennui.Script.Official
{
    public class SafeTypeSet
    {
        public ResourceType Type;
        public int MinTier;
        public int MaxTier;
        public int MinRarity;
        public int MaxRarity;

        public SafeTypeSet() { }
        public SafeTypeSet(int minTier, int maxTier, ResourceType type, int minRarity, int maxRarity)
        {
            this.Type = type;
            this.MinTier = minTier;
            this.MaxTier = maxTier;
            this.MinRarity = minRarity;
            this.MaxRarity = maxRarity;
        }

        public static TypeSet[] BatchConvert(List<SafeTypeSet> set)
        {
            var list = new List<TypeSet>();
            foreach (var t in set)
            {
                list.Add(new TypeSet(t.MinTier, t.MaxTier, t.Type, t.MinRarity, t.MaxRarity));
            }
            return list.ToArray();
        }
    }
}
