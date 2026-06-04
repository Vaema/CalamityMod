using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class SeafoamBombProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public int time = 0;
        public bool notTheClone => Projectile.Calamity().stealthStrike && Projectile.ai[0] == 0; // If the bomb is the initial thrown stealth strike bomb and not a split
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 400;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.direction;
            float dustscaleMult = 1;
            if (notTheClone)
            {
                Projectile.scale = 1.15f;
                Projectile.extraUpdates = 2;
                dustscaleMult = 1.5f;
            }
            if (time > 25)
            {
                Projectile.velocity.Y = Projectile.velocity.Y + 0.1f;
                if (Projectile.velocity.Y > 0)
                Projectile.velocity.X = Projectile.velocity.X * 0.975f;
            }
            Projectile.rotation += Projectile.velocity.X * 0.07f + Projectile.direction * 0.07f;

            Vector2 dustPos = Projectile.Center + (Vector2.UnitY * -22 * Projectile.scale).RotatedBy(Projectile.rotation - (MathHelper.PiOver2 + 0.4f) * Projectile.spriteDirection);
            Dust dust2 = Dust.NewDustPerfect(dustPos, ModContent.DustType<LightDust>(), Projectile.velocity * Main.rand.NextFloat(0.15f, 0.35f));
            dust2.noGravity = true;
            dust2.scale = Main.rand.NextFloat(0.4f, 0.55f) * dustscaleMult;
            dust2.color = Color.Lerp(Color.Cyan, Color.Turquoise, Main.rand.NextFloat(0, 0.7f));
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<LightDust>(), Projectile.velocity.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(0.65f, 0.85f));
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.7f, 1f) * dustscaleMult;
                dust.color = Color.Lerp(Color.Cyan, Color.Turquoise, Main.rand.NextFloat(0, 0.7f));
            }

            time++;
            Lighting.AddLight(Projectile.Center, Color.DodgerBlue.ToVector3() * 0.3f * dustscaleMult);
        }
        public override void OnKill(int timeLeft)
        {
            if (notTheClone) // Split into a few normal bombs that have reduced damage
            {
                for (int i = 0; i < 15; i++)
                {
                    Vector2 randVel = new Vector2(7, 7).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(0.8f, 1.6f);
                    Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, Color.SlateGray * 0.9f, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.9f, 2.3f), 0.4f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                float throwCount = Projectile.localAI[0];
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        throwCount++;
                        Vector2 vel = (Vector2.UnitY * -5).RotatedByRandom(0.6f) * Main.rand.NextFloat(0.8f, 1.3f);
                        int stealth = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, Projectile.type, Projectile.damage / 4, Projectile.knockBack, Projectile.owner);
                        Main.projectile[stealth].Calamity().stealthStrike = true;
                        Main.projectile[stealth].localAI[0] = throwCount;
                        Main.projectile[stealth].ai[0] = 1;
                        Main.projectile[stealth].extraUpdates = 3;
                    }
                }
                
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            }
            else // Normal bubble spawn
            {
                Projectile bubble = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SeafoamBubble>(), Projectile.damage, 0f, Projectile.owner, 0f, 0f, -5f);
                bubble.localAI[0] = Projectile.localAI[0];
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = Main.rand.NextFloat(0.15f, 0.3f), Volume = 0.6f, MaxInstances = 10 }, Projectile.Center);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            if (notTheClone)
            {
                for (int i = 0; i < 25; i++)
                {
                    Color auraColor = Color.Turquoise with { A = 0 } * 0.35f;
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * 3;
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, null, auraColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
                }
            }
            return true;
        }
        public override bool? CanDamage() => (!notTheClone && Projectile.Calamity().stealthStrike && time < 30) ? false : null; // Dont have the cluster bomb splits detonate on enemies right when they spawn
    }
}
