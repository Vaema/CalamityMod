using CalamityMod.Items.Placeables.Crags;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing
{
    internal class CragsBobber : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override string Texture => "CalamityMod/Projectiles/Typeless/SlurperBobber";
        public override void SetDefaults()
        {
            Item.width = 9;
            Item.height = 9;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.fishingSkill += 10;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.FishingBobber).
                AddIngredient<BrimstoneSlag>(3).
                AddIngredient<ScorchedBone>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
