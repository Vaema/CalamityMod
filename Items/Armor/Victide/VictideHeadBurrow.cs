using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Victide
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("VictideHeadgear", "VictideHeadRogue")]
    public class VictideHeadBurrow : ModItem, ILocalizedModType // Mobility/Utility set
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float MoveSpeedAccelerationBoost = 0.15f;
        public static float BaseBurrowSpeed = 9f; // 46 mph
        public static float SubmergedBurrowSpeed = 12f; // 61 mph
        public static float BaseAcceleration = 0.4f; // For reference, base player running acceleration is 0.08
        public static int BurrowDuration = CalamityUtils.SecondsToFrames(8);
        public static int BurrowCooldown = CalamityUtils.SecondsToFrames(30);

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedAccelerationBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5; // 16
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<VictideBreastplate>() && legs.type == ModContent.ItemType<VictideGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            var hotkey = CalamityKeybinds.ArmorSetBonusHotKey.TooltipHotkeyString();
            player.setBonus = this.GetLocalizedValue("SetBonus")
            + "\n" + this.GetLocalization("AbilityBrief").Format(hotkey)
            + "\n" + this.GetLocalization("AbilityDescription").Format(BurrowDuration.FramesToSeconds(), BurrowCooldown.FramesToSeconds());
            player.Calamity().victideBurrowSet = true;
            player.ignoreWater = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.Calamity().victideBurrowHead = true;
            player.moveSpeed += MoveSpeedAccelerationBoost;
            if (player.IsUnderwater())
                player.gills = true;
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
