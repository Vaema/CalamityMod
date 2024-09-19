using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    public class Runestone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.Runestone>());
    }
}
