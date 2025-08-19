using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Umbraphile
{
    [AutoloadEquip(EquipType.Head)]
    public class UmbraphileHood : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static float RogueDamageBoost = 0.08f;
        public static float RogueVelocityBoost = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent(), RogueVelocityBoost.ToPercent());

        // Set Bonus
        public static float SetBonusRogueStealth = 1.1f;
        public static double ExplosionDamageRatio = 0.2D;
        public static int ExplosionDamageSoftcap = 50;

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 10;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<UmbraphileRegalia>() && legs.type == ModContent.ItemType<UmbraphileBoots>();

        public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadow = true;

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.umbraphileSet = true;
            modPlayer.rogueStealthMax += SetBonusRogueStealth;
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusRogueStealth.ToStealth());
            player.Calamity().wearingRogueArmor = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;
            player.Calamity().rogueVelocity += RogueVelocityBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SolarVeil>(12).
                AddIngredient(ItemID.HallowedBar, 8).
                AddTile(TileID.MythrilAnvil).
                SortBeforeFirstRecipesOf(ModContent.ItemType<UmbraphileBoots>()).
                Register();
        }
    }
}
