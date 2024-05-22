using Terraria.DataStructures;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Steamworks;
using Terraria.Audio;
using CalamityMod.Projectiles;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Turbulance : RogueWeapon
    {
        public static SoundStyle LightningStrike = new SoundStyle("CalamityMod/Sounds/Item/TurbulanceLightningStrike");

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.damage = 18;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 18;
            Item.knockBack = 5f;
            Item.autoReuse = true;
            Item.height = 14;
            Item.value = CalamityGlobalItem.Rarity3BuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<TurbulanceProjectile>();
            Item.shootSpeed = 12f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.Calamity().mouseWorldListener = true;

            Vector2 mW = player.Calamity().mouseWorld;

            float value1 = player.direction * -90;
            float value2 = player.direction * 180;
            float value3 = player.direction * -240;

            float g1 = (float)Item.useAnimation;
            float g2 = (float)player.itemAnimation;

            float gg = g2 / g1;

            player.direction = Math.Sign(mW.X - player.Center.X);
            if (player.direction == 0) player.direction = 1;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(mW) + (player.direction == 1 ? MathHelper.ToRadians(180) : 0) + MathHelper.ToRadians(MathHelper.Lerp(value2, value1, CalamityUtils.SineInOutEasing(gg, 1))));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(mW) + (player.direction == 1 ? MathHelper.ToRadians(180) : 0) + MathHelper.ToRadians(MathHelper.Lerp(value1, value3, CalamityUtils.SineInOutEasing(gg, 1))));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.Calamity().mouseWorldListener = true;

            Vector2 SpawnPos = player.MountedCenter + new Vector2(0, -16);
            Vector2 vel = velocity;

            SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing.WithPitchOffset(0.8f), player.MountedCenter);

            if (player.Calamity().StealthStrikeAvailable())
            {
                SoundEngine.PlaySound(WulfrumKnife.Throw1Sound, player.MountedCenter);

                vel *= 0.15f;
            }

            int proj = Projectile.NewProjectile(source, SpawnPos, SpawnPos.DirectionTo(player.Calamity().mouseWorld) * vel.Length(), type, damage, knockback, player.whoAmI);
            Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }

        public override bool CanShoot(Player player)
        {
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AerialiteBar>(7).
                AddIngredient(ItemID.SunplateBlock, 3).
                AddTile(TileID.SkyMill).
                Register();
        }
    }
}
