using System;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class SHPB : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public int explosionTimer = 120;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.scale = 0.4f;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Magic;
        }

        // This is reused for all SHPC projectiles
        #region General SHPC Projectile Functions
        public enum SoulType
        {
            Light, // Larger explosion
            Night, // Explosion lasts longer
            Flight, // Restores flight time on direct hits
            Might, // Launches enemies
            Sight, // Weakly homes
            Fright // Deals extra flat damage
        }

        public static SoulType GetSoulEffects(int projai)
        {
            switch (projai)
            {
                case 0:
                    return SoulType.Light;
                case 1:
                    return SoulType.Night;
                case 2:
                    return SoulType.Flight;
                case 3:
                    return SoulType.Might;
                case 4:
                    return SoulType.Sight;
                case 5:
                    return SoulType.Fright;

                default:
                    return SoulType.Light;
            }
        }

        public static Color FindColorForSoul(int projai)
        {
            switch (projai)
            {
                case 0:
                    return new(240, 29, 196);
                case 1:
                    return new(123, 29, 220);
                case 2:
                    return new(106, 240, 250);
                case 3:
                    return new(4, 51, 222);
                case 4:
                    return new(79, 255, 124);
                case 5:
                    return new(255, 128, 20);

                default:
                    return new(0, 0, 0);
            }
        }
        #endregion General SHPC Projectile Functions

        public override void AI()
        {
            // Light and fade in
            float lights = (float)Main.rand.Next(90, 111) * 0.01f;
            lights *= Main.essScale;
            Lighting.AddLight(Projectile.Center, 1f * lights, 0.2f * lights, 0.75f * lights);
            Projectile.alpha -= 2;

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 0;
            }

            // Size pulsing
            bool lightSoul = GetSoulEffects((int)Projectile.ai[0]) == SoulType.Light;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.scale += 0.05f;
                if (Projectile.scale > 1.9f * (lightSoul ? SHPC.LightExplosionSizeMult : 1f))
                    Projectile.localAI[0] = 1f;
            }
            else
            {
                Projectile.scale -= 0.05f;
                if (Projectile.scale < 1.5f * (lightSoul ? SHPC.LightExplosionSizeMult : 1f))
                    Projectile.localAI[0] = 0f;
            }

            // Sight has weak homing
            if (GetSoulEffects((int)Projectile.ai[0]) == SoulType.Sight)
            {
                float npcDistCheck = SHPC.SightHomingRange;
                int index = -1;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (!n.CanBeChasedBy(Projectile))
                        continue;

                    float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                    if (currentNPCDist < npcDistCheck)
                    {
                        npcDistCheck = currentNPCDist;
                        index = n.whoAmI;
                    }
                }

                if (index != -1)
                {
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Main.npc[index].Center).ToRotation(), 0.1f).ToRotationVector2() * speed;
                }
                else // Slow down over time if not homing
                    Projectile.velocity *= 0.9875f;
            }
            else if (!(GetSoulEffects((int)Projectile.ai[0]) == SoulType.Flight))// Always slow down if not Flight
                Projectile.velocity *= 0.9875f;

            float explodeRange = 250f;
            bool canExplode = false;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.CanBeChasedBy(Projectile, false) && Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1))
                {
                    float npcX = n.position.X + (float)(n.width / 2);
                    float npcY = n.position.Y + (float)(n.height / 2);
                    float npcDist = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - npcX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - npcY);
                    if (npcDist < explodeRange)
                    {
                        explodeRange = npcDist;
                        canExplode = true;
                    }
                }
            }
            if (canExplode)
            {
                explosionTimer--;
                if (explosionTimer <= 0)
                    Projectile.Kill();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            // Flight restores flight time on direct hit
            if (GetSoulEffects((int)Projectile.ai[0]) == SoulType.Flight)
            {
                if (owner.wingTime < owner.wingTimeMax)
                    owner.wingTime += SHPC.FlightDirectHitFlightBoost;

                if (owner.wingTime > owner.wingTimeMax)
                    owner.wingTime = owner.wingTimeMax;
            }

            // Might launches enemies
            if (GetSoulEffects((int)Projectile.ai[0]) == SoulType.Might && target.CanBeMoved(false))
            {
                Vector2 launchVel = (owner.Calamity().mouseWorld - owner.Center).SafeNormalize(Vector2.UnitY) * SHPC.MightKnockbackStrength - new Vector2(0, 3);
                target.velocity = launchVel;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Fright deals extra flat damage
            if (GetSoulEffects((int)Projectile.ai[0]) == SoulType.Fright)
                modifiers.SourceDamage.Flat += SHPC.FrightFlatDamage;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item105, Projectile.Center);
            float screenshake = GetSoulEffects((int)Projectile.ai[0]) == SoulType.Light ? 5f : 3.5f;
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenshake)
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenshake;

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SHPExplosion>(), (int)(Projectile.damage * 1.5f), Projectile.knockBack, Projectile.owner, Projectile.ai[0], 0f);

                for (int i = 0; i < 5; i++)
                {
                    Vector2 soulVelocity = -Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(6f, 9f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, soulVelocity, ModContent.ProjectileType<SHPS>(), (int)(Projectile.damage * 0.33f), 0f, Projectile.owner, Main.rand.Next(6));
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => FindColorForSoul((int)Projectile.ai[0]);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
