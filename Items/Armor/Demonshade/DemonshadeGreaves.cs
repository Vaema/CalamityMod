using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Demonshade
{
    [AutoloadEquip(EquipType.Legs)]
    public class DemonshadeGreaves : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";

        // Shadow Speed... (elusive)
        public static float MoveSpeedBoost = 0.3f;
        public static float AccelerationBoost = 0.5f;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.defense = 50;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.shadowSpeed = true;
            player.moveSpeed += MoveSpeedBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ShadowspecBar>(15).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
