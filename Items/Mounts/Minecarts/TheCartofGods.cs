using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts.Minecarts;

[LegacyName("DoGCart")]
public class TheCartofGods : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Mounts";
    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 36;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item68;
        Item.noMelee = true;
        Item.mountType = ModContent.MountType<DoGCartMount>();

        Item.value = Item.sellPrice(gold: 30);
        Item.rare = ModContent.RarityType<CosmicPurple>();
        Item.Calamity().donorItem = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<CosmiliteBar>(10).
            AddIngredient<AscendantSpiritEssence>().
            AddIngredient(ItemID.Wire, 60).
            AddTile<CosmicAnvil>().
            Register();
    }
}
