using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Sulphurous
{
    [AutoloadEquip(EquipType.Body)]
    [LegacyName("SulfurBreastplate")]
    public class SulphurousBreastplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float RogueDamageBoost = 0.06f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.defense = 6;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateEquip(Player player) => player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(20).
                AddIngredient<SulphuricScale>(20).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
