using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.SnowRuffian
{
    [AutoloadEquip(EquipType.Legs)]
    public class SnowRuffianGreaves : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int RangedCritBoost = 3;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedCritBoost);

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.defense = 3; //9
        }

        public override void UpdateEquip(Player player) => player.GetCritChance<RangedDamageClass>() += RangedCritBoost;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BorealWood, 15).
                AddIngredient(ItemID.Silk, 5).
                AddIngredient(ItemID.FlinxFur).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
