using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Accessories
{
    public class TheAmalgam : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(9, 6));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = RarityType<DarkBlue>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.rBrain = true; // Handles shaderain cloud spawning on hit
            modPlayer.amalgam = true;
            player.brainOfConfusionItem = Item;
            player.GetDamage<GenericDamageClass>() += 0.15f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AmalgamatedBrain>().
                AddIngredient<AscendantSpiritEssence>(4).
                AddIngredient<UnholyEssence>(10).
                AddIngredient<ExodiumCluster>(10).
                AddIngredient<CorrodedFossil>(10).
                AddIngredient<DepthCells>(10).
                AddIngredient<EffulgentFeather>(10).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
