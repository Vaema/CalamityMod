using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Mollusk
{
    [AutoloadEquip(EquipType.Body)]
    public class MolluskShellplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static float DamageBoost = 0.06f;
        public static int CritBoost = 5;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.defense = 18;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
            player.Calamity().molluskChest = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MolluskHusk>(15).
                AddIngredient<SeaPrism>(25).
                AddTile(TileID.Anvils).
                SortBeforeFirstRecipesOf(ModContent.ItemType<MolluskShelleggings>()).
                Register();
        }
    }
}
