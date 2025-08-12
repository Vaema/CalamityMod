using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.FurnitureMonolith;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.TitanHeart
{
    [AutoloadEquip(EquipType.Head)]
    public class TitanHeartMask : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static float RogueDamageBoost = 0.07f;
        public static float RogueVelocityBoost = 0.1f;
        public static int OnHitDebuffDuration = CalamityUtils.SecondsToFrames(2);
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent(), RogueVelocityBoost.ToPercent());

        // Set Bonus
        public static float SetBonusRogueStealth = 1f;
        public static float StealthStrikeKnockbackMult = 2f;
        public static int ExplosionDamage = 40;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 8; // 32
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<TitanHeartMantle>() && legs.type == ModContent.ItemType<TitanHeartBoots>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusRogueStealth.ToStealth(), StealthStrikeKnockbackMult);
            var modPlayer = player.Calamity();
            modPlayer.titanHeartSet = true;
            modPlayer.rogueStealthMax += SetBonusRogueStealth;
            modPlayer.wearingRogueArmor = true;
            player.noKnockback = true;
        }

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.titanHeartMask = true;
            player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;
            modPlayer.rogueVelocity += RogueVelocityBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AstralMonolith>(10).
                AddIngredient<Materials.TitanHeart>().
                AddTile(TileID.Anvils).
                SortBeforeFirstRecipesOf(ModContent.ItemType<TitanHeartMantle>()).
                Register();
        }
    }
}
