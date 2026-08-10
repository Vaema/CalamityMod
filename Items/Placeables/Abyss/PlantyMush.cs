using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Abyss;

public class PlantyMush : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Abyss.PlantyMush>());
        Item.value = Item.sellPrice(copper: 20);
    }

    public override void CaughtFishStack(ref int stack)
    {
        stack = Main.rand.Next(5, 16);
    }
}
