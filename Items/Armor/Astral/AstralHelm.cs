using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Astral
{
    [AutoloadEquip(EquipType.Head)]
    public class AstralHelm : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static float DamageBoost = 0.05f;
        public static int CritBoost = 10;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost);

        // Set Bonus
        public static int SetBonusMinionSlotBoost = 3;
        public static float SetBonusDamageBoost = 0.1f;
        public static int SetBonusCritBoost = 10; // NOTE: Tooltip shares this number with damage % as they're equal
        public static int StarRainCooldown = CalamityUtils.SecondsToFrames(1);
        public static int StarDamage = 120;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
            Item.rare = ItemRarityID.Cyan;
            Item.defense = 17; //63
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<AstralBreastplate>() && legs.type == ModContent.ItemType<AstralLeggings>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusMinionSlotBoost, SetBonusDamageBoost.ToPercent(), StarRainCooldown.FramesToSeconds());
            var modPlayer = player.Calamity();
            modPlayer.astralStarRain = true;
            modPlayer.omniscience = true;
            player.maxMinions += SetBonusMinionSlotBoost;
            player.GetDamage<GenericDamageClass>() += SetBonusDamageBoost;
            player.GetCritChance<GenericDamageClass>() += SetBonusCritBoost;
            player.Calamity().wearingRogueArmor = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AstralBar>(8).
                AddIngredient(ItemID.MeteoriteBar, 6).
                AddTile(TileID.LunarCraftingStation).
                SortBeforeFirstRecipesOf(ModContent.ItemType<AstralBreastplate>()).
                Register();
        }
    }
}
