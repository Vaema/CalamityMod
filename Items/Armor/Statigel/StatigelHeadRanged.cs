using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("StatigelHeadgear")]
    public class StatigelHeadRanged : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float RangedDamageBoost = 0.1f;
        public static int RangedCritBoost = 7;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageBoost.ToPercent(), RangedCritBoost);

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 7; //25
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<StatigelArmor>() && legs.type == ModContent.ItemType<StatigelGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = CalamityUtils.GetTextFromModItem<StatigelArmor>("CommonSetBonus").Format(StatigelArmor.SetBonusJumpSpeedBoost.ToJumpSpeedPercent());
            player.GetJumpState<StatigelJump>().Enable();
            Player.jumpHeight += (int)(StatigelArmor.SetBonusJumpHeightPercentBoost * 15);
            player.jumpSpeedBoost += StatigelArmor.SetBonusJumpSpeedBoost;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<RangedDamageClass>() += RangedDamageBoost;
            player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(8).
                AddIngredient<BlightedGel>(8).
                AddTile(TileID.Solidifier).
                SortBeforeFirstRecipesOf(ModContent.ItemType<StatigelHeadMagic>()).
                Register();
        }
    }
}
