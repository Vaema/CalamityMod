using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class LabHologramProjectorItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LabHologramProjector>());
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ModContent.RarityType<DarkOrange>();
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<LaboratoryPlating>(20).
                AddIngredient<MysteriousCircuitry>(3).
                AddIngredient<DubiousPlating>(3).
                AddIngredient<DraedonPowerCell>(8).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
