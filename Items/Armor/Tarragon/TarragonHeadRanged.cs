using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Tarragon
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("TarragonVisage")]
    public class TarragonHeadRanged : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";

        public static float RangedDamageBoost = 0.1f;
        public static int RangedCritBoost = 7;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageBoost.ToPercent(), RangedCritBoost);

        // Set Bonus
        public static int OnHitEffectCooldown = CalamityUtils.SecondsToFrames(1);
        public static float SetBonusAmmoReduction = 0.75f;
        public static float LeafDamageRatio = 0.25f;
        public static int LeafDamageSoftcap = 150;
        public static float EnergyDamageRatio = 0.33f;
        public static int EnergyDamageSoftcap = 200;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.defense = 28; // 88
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<TarragonBreastplate>() && legs.type == ModContent.ItemType<TarragonLeggings>();

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadowSubtle = true;
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.ammoCost *= SetBonusAmmoReduction;
            modPlayer.tarraSet = true;
            modPlayer.tarraRanged = true;
            player.setBonus = this.GetLocalization("SetBonus").Format((1f - SetBonusAmmoReduction).ToPercent());
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<RangedDamageClass>() += RangedDamageBoost;
            player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UelibloomBar>(7).
                AddIngredient<DivineGeode>(6).
                AddTile(TileID.MythrilAnvil).
                SortBeforeFirstRecipesOf(ModContent.ItemType<TarragonHeadMagic>()).
                Register();
        }
    }
}
