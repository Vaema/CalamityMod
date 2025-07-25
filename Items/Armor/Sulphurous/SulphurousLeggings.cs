using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Sulphurous
{
    [AutoloadEquip(EquipType.Legs)]
    [LegacyName("SulfurLeggings")]
    public class SulphurousLeggings : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float MoveSpeedBoost = 0.1f;
        public static float SubmergedMoveSpeedBoostBuff = 0.25f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBoost.ToPercent(), SubmergedMoveSpeedBoostBuff.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 16;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.defense = 5;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateEquip(Player player) => player.moveSpeed += MoveSpeedBoost + (player.Calamity().countsAsAnyWet ? SubmergedMoveSpeedBoostBuff : 0f);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(15).
                AddIngredient<SulphuricScale>(15).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
