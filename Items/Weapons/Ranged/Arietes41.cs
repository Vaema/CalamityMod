using System;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("Aeries")]
    public class Arietes41 : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public bool swapType = false;
        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 30;
            Item.scale = 0.85f;
            Item.damage = 65;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 13;
            Item.useAnimation = 13;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2.5f;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item41;
            Item.autoReuse = true;
            Item.shootSpeed = 13f;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Bullet;
            Item.Calamity().canFirePointBlankShots = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;

            // Shock bullet
            if (!swapType)
            {
                for (int k = 0; k < 8; k++)
                {
                    SparkParticle spark = new SparkParticle(itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.65f, velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1.5f), false, Main.rand.Next(10, 15 + 1), Main.rand.NextFloat(0.2f, 0.35f), Color.Turquoise);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                Projectile shockShot = Projectile.NewProjectileDirect(source, itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 1.65f, velocity, type, damage, knockback, player.whoAmI);
                CalamityGlobalProjectile cgp = shockShot.Calamity();
                cgp.shockBullet = true;
            }
            // Life bullet
            if (swapType)
            {
                for (int k = 0; k < 8; k++)
                {
                    SparkParticle spark = new SparkParticle(itemPosition + velocity.RotatedBy(-0.6 * player.direction) + velocity * 0.65f, velocity.RotatedByRandom(0.25) * Main.rand.NextFloat(0.2f, 1.5f), false, Main.rand.Next(10, 15 + 1), Main.rand.NextFloat(0.2f, 0.35f), Color.White);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                Projectile lifeShot = Projectile.NewProjectileDirect(source, itemPosition + velocity.RotatedBy(-0.6 * player.direction) - velocity * 0.65f, velocity, type, damage, knockback, player.whoAmI);
                CalamityGlobalProjectile cgp = lifeShot.Calamity();
                cgp.lifeBullet = true;
            }

            swapType = !swapType;
            return false;
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;
            Vector2 itemSize = new Vector2(56, 30);
            Vector2 itemOrigin = new Vector2(-24, 3);

            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);

            base.UseStyle(player, heldItemFrame);
        }

        public override void UseItemFrame(Player player)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

            float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
            float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
            if (animProgress < 0.4f)
                rotation += -0.03f * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ThermoclineBlaster>().
                AddIngredient(ItemID.ShroomiteBar, 3).
                AddIngredient(ItemID.FallenStar, 3). // 41 Arietis is a triple star system
                AddIngredient<CoreofSunlight>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
