using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel;

[AutoloadEquip(EquipType.Head)]
[LegacyName("StatigelCap")]
public class StatigelHeadMagic : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PreHardmode";

    public static int MaxManaBoost = 40;
    public static float ManaCostReduction = 0.1f;
    public static float MagicDamageBoost = 0.1f;
    public static int MagicCritBoost = 7;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaBoost, ManaCostReduction.ToPercent(), MagicDamageBoost.ToPercent(), MagicCritBoost);

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 5; //22
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
        player.GetDamage<MagicDamageClass>() += MagicDamageBoost;
        player.GetCritChance<MagicDamageClass>() += MagicCritBoost;
        player.manaCost -= ManaCostReduction;
        player.statManaMax2 += MaxManaBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PurifiedGel>(8).
            AddIngredient<BlightedGel>(8).
            AddTile(TileID.Solidifier).
            SortBeforeFirstRecipesOf(ModContent.ItemType<StatigelArmor>()).
            Register();
    }
}
