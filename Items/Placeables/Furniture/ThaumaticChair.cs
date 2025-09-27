using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.FurnitureAbyss;
using CalamityMod.Items.Placeables.FurnitureAcidwood;
using CalamityMod.Items.Placeables.FurnitureAncient;
using CalamityMod.Items.Placeables.FurnitureAshen;
using CalamityMod.Items.Placeables.FurnitureBotanic;
using CalamityMod.Items.Placeables.FurnitureCosmilite;
using CalamityMod.Items.Placeables.FurnitureNavystone;
using CalamityMod.Items.Placeables.FurnitureExo;
using CalamityMod.Items.Placeables.FurnitureMarnite;
using CalamityMod.Items.Placeables.FurnitureMonolith;
using CalamityMod.Items.Placeables.FurnitureOtherworldly;
using CalamityMod.Items.Placeables.FurniturePlagued;
using CalamityMod.Items.Placeables.FurnitureProfaned;
using CalamityMod.Items.Placeables.FurnitureSacrilegious;
using CalamityMod.Items.Placeables.FurnitureSilva;
using CalamityMod.Items.Placeables.FurnitureStatigel;
using CalamityMod.Items.Placeables.FurnitureStratus;
using CalamityMod.Items.Placeables.FurnitureVoid;
using CalamityMod.Items.Placeables.FurnitureWulfrum;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class ThaumaticChair : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ThaumaticChairTile>());
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.Calamity().donorItem = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient<AbyssChair>().
            AddIngredient<AcidwoodChair>().
            AddIngredient<AncientChair>().
            AddIngredient<AshenChair>().
            AddIngredient<BotanicChair>().
            AddIngredient<CosmiliteChair>().
            AddIngredient<NavystoneChair>().
            AddIngredient<ExoChair>().
            AddIngredient<MarniteChair>().
            AddIngredient<MonolithChair>().
            AddIngredient<SacrilegiousChair>().
            AddIngredient<OtherworldlyChair>().
            AddIngredient<PlaguedPlateChair>().
            AddIngredient<ProfanedChair>().
            AddIngredient<SilvaChair>().
            AddIngredient<StatigelChair>().
            AddIngredient<StratusChair>().
            AddIngredient<VoidChair>().
            AddIngredient<WulfrumChair>().
            AddIngredient<AuricBar>().
            AddTile<CosmicAnvil>().
            Register();
        }
    }
}
