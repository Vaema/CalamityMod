using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class AsgardsValor : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";

        public Color? TooltipExtensionColor => new(195, 223, 255);

        public const int ShieldSlamDamage = 200;
        public const float ShieldSlamKnockback = 9f;
        public const int ShieldSlamIFrames = 12;

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 48;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 4;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            // Asgard's Valor ram dash
            modPlayer.DashID = AsgardsValorDash.ID;
            player.dashType = 0;

            // Inherited Obsidian Shield effects
            player.noKnockback = true;
            player.fireWalk = true;

            player.buffImmune[BuffID.OnFire] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.ObsidianShield).
                AddIngredient<OrnateShield>().
                AddIngredient<CoreofCalamity>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
