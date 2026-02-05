using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
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

        public static int NimbusDamage => CalamityUtils.ScaleWithDifficulty(200);

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
            Item.rare = RarityType<CosmicPurple>();
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.rBrain = true; // Handles shaderain cloud spawning on hit
            modPlayer.amalgam = true;
            player.brainOfConfusionItem = Item;
            modPlayer.HeatDebuffMultiplier += 3f;
            modPlayer.ColdDebuffMultiplier += 3f;
            modPlayer.SicknessDebuffMultiplier += 3f;
            modPlayer.WaterDebuffMultiplier += 3f;
            modPlayer.ElectricDebuffMultiplier += 3f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AmalgamatedBrain>().
                AddIngredient<AscendantSpiritEssence>(4).
                AddIngredient(ItemID.LunarBar, 10).
                AddIngredient(ItemID.FragmentSolar, 10).
                AddIngredient<PlagueCellCanister>(15).
                AddIngredient<DepthCells>(15).
                AddIngredient<EffulgentFeather>(8).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
