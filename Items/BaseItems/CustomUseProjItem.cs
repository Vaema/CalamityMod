using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.BaseItems
{
    public abstract class CustomUseProjItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.channel = true;
        }

        public override bool CanShoot(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
    }
}
