using System;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
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

        public static int RegenBoost = 2;
        public static float RunAccelerationMult = 1.75f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond(), RunAccelerationMult.Round());

        // Set Bonus
        public static int BarrierCooldown = CalamityUtils.SecondsToFrames(8);
        public static int BarrierDefenseBoost = 6;
        public static float BarrierDamageAbsorptionPercent = 0.1f;
        public static int BarrierFramesPerHeal = 15;
        public static int BarrierDamage = 50;
        public static float BarrierExplosionKB = 8f;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5; // 12
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<VictideBreastplate>() && legs.type == ModContent.ItemType<VictideGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            Color AbilityBriefColor = Color.Lerp(new Color(97, 200, 255), new Color(255, 170, 204), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f));
            player.setBonus = this.GetLocalization("SetBonus").Format(AbilityBriefColor.Hex3(), BarrierDefenseBoost, BarrierDamageAbsorptionPercent.ToPercent(), CalamityUtils.GetArmorSetBonusKey(), BarrierCooldown.FramesToSeconds());
            player.Calamity().victideBarrierSet = true;

            if (!player.HasCooldown(WardingWave.ID))
            {
                player.statDefense += BarrierDefenseBoost;

                if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[ModContent.ProjectileType<VictideBarrier>()] < 1)
                {
                    // Damage will be set once explosion goes off
                    var source = player.GetSource_ItemUse(Item);
                    Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<VictideBarrier>(), 0, 0f, player.whoAmI);
                }
            }
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
