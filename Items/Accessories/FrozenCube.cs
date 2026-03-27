using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Accessories
{
    public class FrozenCube : ModItem, ILocalizedModType
    {
        public static int mistBaseDamage = 3;
        public static int slamBaseDamage = 24;
        public static int baseAttackSpeed = 90;
        public static int baseAttackCooldown = 180;
        public static int debuff = BuffID.Frostburn; // Wind chilled
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.frozenCube = true;
            modPlayer.ColdDebuffMultiplier += modPlayer.frozenCubePower;

            int projectile = ProjectileType<Elumphant>();
            if (player.ownedProjectileCounts[projectile] < 1 && !player.dead)
            {
                Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, projectile, 0, 0f, player.whoAmI);
            }
        }
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            Player player = Main.LocalPlayer;
            if (Main.LocalPlayer != null)
                list.FindAndReplace("[DAMAGE]", ((int)(player.Calamity().frozenCubePower)).ToString() + "x");
        }
    }
}
