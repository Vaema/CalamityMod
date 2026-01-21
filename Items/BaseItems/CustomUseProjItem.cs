using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.BaseItems
{
    public abstract class CustomUseProjItem : ModItem
    {
        public override void SetDefaults()
        {

        }

        public override bool CanShoot(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
    }
}
