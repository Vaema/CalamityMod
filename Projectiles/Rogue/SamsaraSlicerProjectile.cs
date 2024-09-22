using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class SamsaraSlicerProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/SamsaraSlicer";

        public float ReboundVelocity => 20;
        public float StealthReboundVelocity => 30;
        public int ReboundTime => 20;

        Vector2 oldVelocity;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 46;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.penetrate = -1;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.aiStyle = -1;
            Projectile.ai[0] = -200;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Main movement

            if (Projectile.ai[0] < 0)
            {
                Projectile.ai[1]++;

                if (Projectile.ai[1] > ReboundTime)
                {
                    float lerp = (float)(Projectile.ai[1] - ReboundTime) * 0.01f;

                    if (Projectile.Calamity().stealthStrike)
                        lerp = (float)(Projectile.ai[1] - ReboundTime) * 0.005f;

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(player.Center) * (Projectile.Calamity().stealthStrike ? StealthReboundVelocity : ReboundVelocity), lerp);

                    if (Projectile.Distance(player.Center) < Projectile.velocity.Length() * 1.4f)
                    {
                        Projectile.Kill();
                    }
                }
            }

            // Frame pause

            Projectile.ai[0]--;

            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity = oldVelocity;

                SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot.WithPitchOffset(1f));

                for (int i = 1; i <= 2; i++)
                {
                    GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, new Vector2(i == 1 ? 2 : 6, 0).RotatedBy(Projectile.velocity.ToRotation()), Color.LimeGreen, "CalamityMod/Particles/BloomRing", new Vector2(0.5f, 1f), Projectile.velocity.ToRotation(), 0.1f, 0.5f - (i * 0.1f), 20));
                }
                for (int i = 0; i <= 5; i++)
                {
                    GeneralParticleHandler.SpawnParticle(new CustomSpark(Projectile.Center, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-10f, 10f)).RotatedBy(Projectile.velocity.ToRotation()), "CalamityMod/Particles/ThinEndedLine", false, 10, Main.rand.NextFloat(0.3f, 1f), Main.rand.NextBool() ? new Color(1f, 0.8f, 0.1f) : Color.LimeGreen, new Vector2(Main.rand.NextFloat(0.4f, 1f), 1f)));
                }
            }
            
            if (Projectile.ai[0] <= 0 && Projectile.ai[0] > -4)
            {
                Projectile.extraUpdates = 1;
            }
            else
            {
                Projectile.extraUpdates = 0;
            }

            Vector2 vel = Projectile.velocity;
            if (Projectile.ai[0] > 0)
            {
                vel = oldVelocity * 1.5f;
            }

            Projectile.rotation += MathHelper.ToRadians(vel.Length() * 1.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, tex.Frame(), Color.White, Projectile.rotation, tex.Frame().Center(), 1f, SpriteEffects.None);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], new Color(0f, 0.6f, 0f, 0f), 2, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Rogue/SamsaraSlicerGlow").Value);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // TODO: Latch effects
            if (Projectile.ai[0] <= -200)
                oldVelocity = Projectile.velocity * 2f;
            else
                oldVelocity = Projectile.velocity;
            Projectile.ai[1] = ReboundTime - 15;
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0] = 5;

            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);
        }

        // Make it bounce on tiles.
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Impacts the terrain even though it bounces off.
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.ai[0] = 1f;
            return false;
        }
    }
}
