using CalamityMod.ExtraJumps;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("StatigelHelm")]
    public class StatigelHeadMelee : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float MeleeDamageBoost = 0.1f;
        public static float MeleeSpeedBoost = 0.1f; // NOTE: Tooltip shares this number with damage % as they're equal
        public static int MeleeCritBoost = 7;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MeleeDamageBoost.ToPercent(), MeleeCritBoost);

        // Set Bonus
        public static int SetBonusAggroBoost = 400;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 9; //27
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<StatigelArmor>() && legs.type == ModContent.ItemType<StatigelGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalizedValue("SetBonus")
            + "\n" + CalamityUtils.GetTextFromModItem<StatigelArmor>("CommonSetBonus").Format(StatigelArmor.SetBonusHurtDamageThreshold, StatigelArmor.SetBonusJumpSpeedBoost.ToJumpSpeedPercent());
            var modPlayer = player.Calamity();
            modPlayer.statigelSet = true;
            player.GetJumpState<StatigelJump>().Enable();
            Player.jumpHeight += (int)(StatigelArmor.SetBonusJumpHeightPercentBoost * 15);
            player.jumpSpeedBoost += StatigelArmor.SetBonusJumpSpeedBoost;
            player.aggro += SetBonusAggroBoost;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<MeleeDamageClass>() += MeleeDamageBoost;
            player.GetCritChance<MeleeDamageClass>() += MeleeCritBoost;
            player.GetAttackSpeed<MeleeDamageClass>() += MeleeSpeedBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(5).
                AddIngredient<BlightedGel>(5).
                AddTile(TileID.Solidifier).
                SortBeforeFirstRecipesOf(ModContent.ItemType<StatigelHeadMagic>()).
                Register();
        }
    }
}
