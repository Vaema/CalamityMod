using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Victide
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("VictideMask", "VictideHeadMagic")]
    public class VictideHeadBarrier : ModItem, ILocalizedModType // Defense/Mobility set
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int RegenBoost = 3;
        public static float RunAccelerationMult = 1.75f;
        public static int BarrierCooldown = CalamityUtils.SecondsToFrames(8);
        public static int BarrierDefenseBoost = 10;
        public static int BarrierDefenseDamageRecovery = 2;
        public static int BarrierExplosionDamage = 100;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), RunAccelerationMult.Round());

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 9; // 20
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<VictideBreastplate>() && legs.type == ModContent.ItemType<VictideGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            var hotkey = CalamityKeybinds.ArmorSetBonusHotKey.TooltipHotkeyString();
            player.setBonus = this.GetLocalizedValue("AbilityBrief")
            + "\n" + this.GetLocalization("AbilityDescription").Format(BarrierDefenseBoost, BarrierDefenseDamageRecovery, BarrierCooldown.FramesToSeconds(), hotkey);
            player.Calamity().victideBarrierSet = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.lifeRegen += RegenBoost;
            player.Calamity().victideBarrierHead = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SeaRemains>(3).
                AddTile(TileID.Anvils).
                SortBeforeFirstRecipesOf(ModContent.ItemType<VictideBreastplate>()).
                Register();
        }
    }
}
