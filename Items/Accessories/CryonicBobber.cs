using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing
{
    internal class CryonicBobber : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override string Texture => "CalamityMod/Projectiles/Typeless/VerstaltiteBobber";
        public override void SetDefaults()
        {
            Item.width = 9;
            Item.height = 9;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
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
                AddIngredient<CryonicBar>(2).
                AddIngredient(ItemID.CrystalShard).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
