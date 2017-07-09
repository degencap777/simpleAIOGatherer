using Ennui.Api;
using Ennui.Api.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ennui.Script.Official
{
    public static class EquipmentUtils
    {
        public static bool HasBrokenItems(this IApi api)
        {
            var local = api.Players.LocalPlayer;
            if (local == null)
                return false;

            var inventory = local.InventoryContainer;
            if (inventory == null)
                return false;

            var items = inventory.ValidItems;
            if (items == null)
                return false;

            foreach (var item in items)
            {
                if (item.Integrity <= 10)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
