using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class SeashineSword : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.damage = 55;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Summon;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.knockBack = 1f;
            Item.shootSpeed = 12f;
            Item.mana = 10;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<SeashineSwordProj>();
        }
        public override bool AltFunctionUse(Player player) => true;
        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 0.2f;
            else
                return 1f;
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                int blades = 0;
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    Projectile projectile = Main.projectile[x];
                    if (projectile.active && projectile.type == Item.shoot && projectile.ai[0] < 5 && projectile.timeLeft < 90000 - 180)
                    {
                        projectile.ai[0] = 5;
                        blades++;
                    }
                }
                if (blades > 0)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(player.Center, ModContent.DustType<LightDust>(), (velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(5f, 10f));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.95f, 1.7f);
                        dust.color = Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue;
                        dust.noLightEmittence = true;

                        Particle fx = new CustomSpark(player.Center, (velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(10f, 20f), "CalamityMod/Particles/Sparkle", false, (int)(Main.rand.Next(16, 26 + 1)), Main.rand.NextFloat(1.1f, 1.7f), Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue, new Vector2(0.5f, 1.1f), extraRotation: 0, shrinkSpeed: 0.2f);
                        GeneralParticleHandler.SpawnParticle(fx);
                    }
                    SoundEngine.PlaySound(SoundID.Item101 with { Volume = 0.9f, Pitch = Main.rand.NextFloat(0.3f, 0.5f) }, player.Center);
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item1, player.Center);

                int pr = Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(pr))
                    Main.projectile[pr].originalDamage = Item.damage;

                float angleMax = MathHelper.ToRadians(360f);
                if (CalamityUtils.CountProjectiles(type) == 1)
                    angleMax = 0f;
                float index = 1f;
                if (player.ownedProjectileCounts[Item.shoot] > 30)
                {
                    angleMax += MathHelper.ToRadians((player.ownedProjectileCounts[Item.shoot] - 30) * 2.5f);
                }
                angleMax = angleMax > MathHelper.ToRadians(360f) ? MathHelper.ToRadians(360f) : angleMax; // More intuative than using a min function
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type == type && p.owner == player.whoAmI)
                    {
                        p.ai[2] = (index / CalamityUtils.CountProjectiles(type)) * angleMax - angleMax / 2f;
                        p.ai[1] = index;
                        p.netUpdate = true;
                        index++;
                    }
                }
            }
            return false;
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.altFunctionUse == 2)
            {
                player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
                float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

                float pullback = 7f;

                float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
                float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
                if (animProgress < 0.4f)
                    pullback -= (2.75f) * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2);

                Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * pullback;
                Vector2 itemSize = new Vector2(52, 28);
                Vector2 itemOrigin = new Vector2(-24, 4);

                CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);

                base.UseStyle(player, heldItemFrame);
            }
        }

        public override void UseItemFrame(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

                float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
                float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
                if (animProgress < 0.4f)
                    rotation += (-0.15f) * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PearlShard>(3).
                AddIngredient<SeaPrism>(7).
                AddIngredient<Navystone>(10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
