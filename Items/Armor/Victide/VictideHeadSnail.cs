using System;
using CalamityMod.Buffs.Summon;
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
    [LegacyName("VictideHelmet", "VictideHeadSummon")]
    public class VictideHeadSnail : ModItem, ILocalizedModType // Utility/Fishing set
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int FishingPowerBoost = 5;
        public static int AggroReduction = 200;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FishingPowerBoost);

        // Set Bonus
        public static int SetBonusGrabRangeBoost = 102; // (2.625 + 6.375 = 9 tiles)

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 1; // 8
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<VictideBreastplate>() && legs.type == ModContent.ItemType<VictideGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            Color AbilityBriefColor = Color.Lerp(new Color(97, 200, 255), new Color(255, 170, 204), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f));
            player.setBonus = this.GetLocalization("SetBonus").Format(AbilityBriefColor.Hex3());
            player.Calamity().victideSnailSet = true;
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.FindBuffIndex(ModContent.BuffType<SeaSnailBuff>()) == -1)
                {
                    player.AddBuff(ModContent.BuffType<SeaSnailBuff>(), 3600, true);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<VictideSeaSnail>()] < 1)
                {
                    var source = player.GetSource_ItemUse(Item);
                    Vector2 position = player.Center + Vector2.UnitY * (player.gfxOffY - 60f) * player.gravDir;
                    Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<VictideSeaSnail>(), 0, 0f, player.whoAmI);
                }
            }
        }

        public override void UpdateEquip(Player player)
        {
            player.fishingSkill += FishingPowerBoost;
            player.aggro -= AggroReduction;
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
