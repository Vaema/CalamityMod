using System;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
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
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedAccelerationBoost.ToPercent());

        // Set Bonus
        public static float BaseBurrowSpeed = 8f; // 41 mph
        public static float SubmergedBurrowSpeed = 10f; // 51 mph
        public static float BaseAcceleration = 0.4f; // For reference, base player running acceleration is 0.08
        public static int BurrowDuration = CalamityUtils.SecondsToFrames(5);
        public static int BurrowCooldown = CalamityUtils.SecondsToFrames(30);

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 3; // 10
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<VictideBreastplate>() && legs.type == ModContent.ItemType<VictideGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            Color AbilityBriefColor = Color.Lerp(new Color(97, 200, 255), new Color(255, 170, 204), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f));
            player.setBonus = this.GetLocalization("SetBonus").Format(AbilityBriefColor.Hex3(), CalamityUtils.GetArmorSetBonusKey(), BurrowDuration.FramesToSeconds(), BurrowCooldown.FramesToSeconds());
            player.Calamity().victideBurrowSet = true;
            player.ignoreWater = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.Calamity().victideBurrowHead = true;
            player.moveSpeed += MoveSpeedAccelerationBoost;
            if (player.Calamity().countsAsAnyWet)
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
