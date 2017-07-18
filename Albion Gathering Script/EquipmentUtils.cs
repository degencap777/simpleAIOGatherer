using Ennui.Api;

namespace Ennui.Script.Official
{
    public static class EquipmentUtils
    {
        public static bool HasBrokenItems(this IApi api)
        {
            var equipment = api.Equipment.AllItems;
            if (equipment != null)
            {
                foreach (var item in equipment)
                {
                    if (item.Integrity <= 10)
                    {
                        return true;
                    }
                }
            }

            var inventory = api.Inventory.AllItems;
            if (inventory != null)
            {
                foreach (var item in inventory)
                {
                    if (item.Integrity <= 10)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
