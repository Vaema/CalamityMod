using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class MoltenAmputatorProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/MoltenAmputator";

        public int time = 0;
        public Vector2 squash = new Vector2(1, 1);
        public float fakeRot = 0;
        public int returnTime = 300;
        public bool pulled = false;
        public bool returning = false;
        public int direction = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 74;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 3 * Projectile.MaxUpdates;
            Projectile.timeLeft = 900;
            Projectile.extraUpdates = 2;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        private void SpawnBlobs(int blobCount)
        {
            for (int i = 0; i < blobCount; i++)
            {
                Vector2 iAmSpeed = new Vector2((float)Main.rand.Next(-100, 101), (float)Main.rand.Next(-100, 101));
                while (iAmSpeed.X == 0f && iAmSpeed.Y == 0f)
                {
                    iAmSpeed = new Vector2((float)Main.rand.Next(-100, 101), (float)Main.rand.Next(-100, 101));
                }
                iAmSpeed.Normalize();
                iAmSpeed *= (float)Main.rand.Next(70, 101) * 0.1f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, iAmSpeed.X, iAmSpeed.Y, ModContent.ProjectileType<MoltenBlobThrown>(), (int)(Projectile.damage * 0.25), 0f, Projectile.owner, 0f, 0f);
            }
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];

            fakeRot += 0.13f * direction;
            Projectile.rotation = (Projectile.velocity.ToRotation()) + MathHelper.PiOver2;

            float x = MathHelper.Clamp(Utils.GetLerpValue(16, 2, Projectile.velocity.Length(), true), 0.3f, 1);
            float y = 1;
            squash = new Vector2(x, y);

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 7)
            {
                Projectile.frame = 0;
            }
            /*
            
            // Stealth strikes constantly spawn molten blobs.
            if (Projectile.Calamity().stealthStrike)
            {
                // If the stealth blob timer isn't set up yet, set it up
                if (Projectile.ai[1] == 0f)
                    Projectile.ai[1] = FramesPerBlob;
                else
                {
                    Projectile.ai[1]--;
                    if (Projectile.ai[1] <= 0f)
                    {
                        SpawnBlobs(1);
                        Projectile.ai[1] = FramesPerBlob;
                    }
                }
            }
            */
            // Frame 1, pick a direction for the scythe. This direction isn't changed from that point on
            if (direction == 0f)
            {
                direction = (Utils.DirectionTo(Projectile.Center, Owner.Calamity().mouseWorld).X > 0 ? 1 : -1);
            }

            if (time >= returnTime && Projectile.ai[2] == 5)
                pulled = true;
            else
                Projectile.ai[2] = 0;

            // Boomerang glows orange
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 1.5f);

            // Boomerang noises
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 7 * Projectile.MaxUpdates;
                SoundStyle sound = new("CalamityMod/Sounds/Item/SwooshMid");
                SoundEngine.PlaySound(sound, Projectile.Center);
            }

            // Main boomerang logic. projectile.ai[0] is a frame counter.
            Projectile.ai[0] += 1f;

            // On the first returning frame, send a net update.
            if (time == returnTime)
                Projectile.netUpdate = true;

            // Once returning, use boomerang return AI.
            if (time >= returnTime)
            {
                Vector2 moveToTrackingPos = (Owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                if (Projectile.velocity.Length() < 3 + (8 * Utils.GetLerpValue(returnTime * 1.2f, returnTime * 1.5f, time, true)))
                    Projectile.velocity += moveToTrackingPos * (0.02f + (4 * Utils.GetLerpValue(returnTime * 1.2f, returnTime * 2.5f, time, true)));
                else
                    Projectile.velocity *= 0.95f;

                // Destroy the boomerang when it returns to the player.
                if (Main.myPlayer == Projectile.owner)
                    if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                        Projectile.Kill();
            }
            else
                Projectile.velocity *= (time > returnTime * 0.4f ? 0.9f : 0.982f);

            if (Main.rand.NextBool())
            {
                int numParts = 2;
                for (int i = 0; i < numParts; i++)
                {
                    float fade = (Utils.GetLerpValue(5, 2, Projectile.velocity.Length(), true) * 3 + 1) * squash.X;

                    float rot = fakeRot + (MathHelper.TwoPi * i / numParts);
                    Vector2 vel = (Utils.MoveTowards(-Projectile.velocity, new Vector2(0, -130).RotatedBy(rot).RotatedBy(-1.3f * direction), (Utils.GetLerpValue(5, 2, Projectile.velocity.Length(), true))));
                    
                    if (Main.rand.NextBool(6))
                    {
                        Particle spark2 = new CustomSpark(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), vel.RotatedByRandom(0.4f) * fade, "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(1.15f, 1.3f), Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f)), new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.3f, 0.4f));
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                    else
                    {
                        Particle spark = new CustomSpark(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), vel.RotatedByRandom(0.4f) * fade, "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(0.75f, 0.82f), Main.rand.NextBool(4) ? Color.Khaki : Color.Orange, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.3f, 0.4f));
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    if (Main.rand.NextBool(6))
                    {
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), Main.rand.NextBool(4) ? 278 : 267);
                        dust2.noGravity = (dust2.type == 278 ? false : true);
                        dust2.scale = dust2.type == 278 ? 0.95f : 1.2f;
                        dust2.color = Color.Lerp(Color.Orchid, Color.White, Main.rand.NextFloat(0, 0.7f));
                        dust2.velocity = (vel * 2).RotatedByRandom(0.4f) * fade;
                    }
                    else
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, -70 * squash.X).RotatedBy(rot), Main.rand.NextBool(4) ? 278 : 267);
                        dust.noGravity = (dust.type == 278 ? false : true);
                        dust.scale = dust.type == 278 ? 0.75f : 0.9f;
                        dust.color = Main.rand.NextBool(4) ? Color.Khaki : Color.Goldenrod;
                        dust.velocity = (vel * 2).RotatedByRandom(0.4f) * fade;
                    }
                }
            }

            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            /*
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);
            if (Projectile.owner == Main.myPlayer)
            {
                int blobCount = Projectile.Calamity().stealthStrike ? 4 : 2;
                SpawnBlobs(blobCount);
            }
            SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
            for (int k = 0; k < 10; k++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.CopperCoin, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
            }
            */
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation;
            float fade = Utils.GetLerpValue(4, 2, Projectile.velocity.Length(), true);

            Asset<Texture2D> p = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire2");
            Asset<Texture2D> p2 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire3");
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(p2.Value, drawPosition, null, Color.Orchid with { A = 0 } * 0.25f * fade, fakeRot * (Main.rand.NextFloat(1.5f, 1.55f) * (i * 0.5f + 0.2f)), p2.Size() * 0.5f, 1.1f * Main.rand.NextFloat(0.8f, 1.15f) * fade, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                Main.EntitySpriteDraw(p.Value, drawPosition, null, Color.Orange with { A = 0 } * 0.35f * fade, fakeRot * (Main.rand.NextFloat(1.1f, 1.15f) * (i * 0.5f + 0.2f)), p.Size() * 0.5f, 0.9f * fade, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }

            Asset<Texture2D> tex3 = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/MoltenAmputatorSHIT");
            Rectangle frame = tex3.Frame(1, 8, 0, Projectile.frame);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(tex3.Value, drawPosition, frame, lightColor, drawRotation, rotationPoint, squash * Projectile.scale, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            //Projectile.DrawProjectileWithBackglow(Color.Goldenrod with { A = 0 } * fade, lightColor, 5.5f * fade, tex3.Value, frame, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, squash * Projectile.scale);
            return false;
        }
    }
}
