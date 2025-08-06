using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class OrnateShield : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public const int ShieldSlamDamage = 50;
        public const float ShieldSlamKnockback = 3f;
        public const int ShieldSlamIFrames = 12;

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.defense = 2; // This has a ram dash, it should give a bit less defense due to how good it is
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Ornate Shield ram dash
            player.Calamity().DashID = OrnateShieldDash.ID;
            player.dashType = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CryonicBar>(5).
                AddIngredient(ItemID.CrystalShard, 10).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
