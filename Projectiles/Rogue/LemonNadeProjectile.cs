using System;
using System.Reflection.Metadata;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Packets.Entities;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class LemonNadeProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.ai[2] == 1)
            {
                Projectile.idStaticNPCHitCooldown = 2;
                Projectile.usesIDStaticNPCImmunity = true;
                Projectile.damage = 0;
            }

            Projectile.rotation += 0.175f * Projectile.direction;

            if (Projectile.timeLeft < 280 && Projectile.ai[2] != 1)
                Projectile.velocity.Y += 0.22f;

            Projectile.ai[0]++;
            //Copied from violence/chalice
            if (Main.rand.NextFloat() < Projectile.ai[0] / Projectile.ai[1] && Projectile.FinalExtraUpdate())
            {
                int bloodLifetime = Main.rand.Next(5, 15);
                float bloodScale = Main.rand.NextFloat(0.6f, 0.8f);
                Color bloodColor = Color.Lerp(Color.Yellow, Color.Goldenrod, Main.rand.NextFloat());
                bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                float randomSpeedMultiplier = Main.rand.NextFloat(1.25f, 2.25f);
                Vector2 bloodVelocity = Main.rand.NextVector2Unit()  * randomSpeedMultiplier;
                bloodVelocity.Y -= 5f;
                BloodParticle blood = new BloodParticle(Projectile.Center, bloodVelocity.RotatedBy(Projectile.rotation + MathHelper.PiOver4 * Projectile.spriteDirection), bloodLifetime, bloodScale, bloodColor);
                GeneralParticleHandler.SpawnParticle(blood);
            }

            if (Projectile.ai[0] > Projectile.ai[1] || Projectile.timeLeft == 2)
            {
                if (Projectile.ai[2] == 1)
                    Projectile.damage = Projectile.originalDamage;
                if (Projectile.Calamity().stealthStrike)
                {
                    var frags = 6f;
                    for (var i = 0; i < frags; i++)
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY.RotatedBy(i/frags * MathHelper.TwoPi) * -10, ModContent.ProjectileType<LemonNadeProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 20,1);
                }
                Projectile.Resize(360, 360);
                Projectile.ResetLocalNPCHitImmunity();
                Projectile.damage *= 10;
                Projectile.Damage();

                for (var i = 0; i < 40; i++)
                {
                    var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LemonNadeExplodeDust>(),Main.rand.NextVector2CircularEdge(15,15) * Main.rand.NextFloat(0.25f,1f),Scale:Main.rand.NextFloat(0.75f,1.25f));
                }
                var exactBloodCount = 10;
                // Code copied from Violence/chalice.
                float bloodVelMult = 6;
                for (int i = 0; i < exactBloodCount; ++i)
                {
                    int bloodLifetime = Main.rand.Next(22, 36);
                    float bloodScale = Main.rand.NextFloat(0.6f, 1.2f);
                    Color bloodColor = Color.Lerp(Color.Yellow, Color.Goldenrod, Main.rand.NextFloat());
                    bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                    float randomSpeedMultiplier = Main.rand.NextFloat(1.25f, 2.25f);
                    Vector2 bloodVelocity = Main.rand.NextVector2Unit() * bloodVelMult * randomSpeedMultiplier;
                    bloodVelocity.Y -= 5f;
                    BloodParticle blood = new BloodParticle(Projectile.Center, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                    GeneralParticleHandler.SpawnParticle(blood);
                }

                SoundEngine.PlaySound(SoundID.Item62 with {pitch = 1f },Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item111 with { pitch = 0.5f }, Projectile.Center);
                Projectile.Kill();

            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[2] == 1)
            {
                Main.instance.LoadItem(ItemID.PumpkinSeed);
                var tex = TextureAssets.Item[ItemID.PumpkinSeed];
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.velocity.ToRotation() - MathHelper.PiOver2, tex.Size() * 0.5f, Projectile.scale,0);
                return false;
            }
            return true;
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.SourceDamage *= 0.002f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.velocity.X *= -0.65f;
            Projectile.velocity.Y = -3f;
            if (Projectile.ai[1] - Projectile.ai[0] < 30)
                Projectile.ai[0] += 15;
        }


        // Make it bounce on tiles.
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Impacts the terrain even though it bounces off.
            if (Projectile.width < 100)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.velocity *= 0.75f;
            return false;
        }
    }

}
