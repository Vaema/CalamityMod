using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(new EquipType[] { EquipType.HandsOn, EquipType.HandsOff })]
    public class BloodstainedGlove : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 36;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.bloodyGlove = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.CrimtaneBar, 2).
                AddIngredient<BloodSample>(6).
                AddIngredient(ItemID.Vertebrae, 2).
                AddTile(TileID.DemonAltar).
                Register();
        }
    }
}
