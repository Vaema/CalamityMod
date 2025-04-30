using System;
using System.Collections.Generic;
using System.Threading;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SHPV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private Player Owner => Main.player[Projectile.owner];
        public ref float Timer => ref Projectile.ai[0];

        // The maximum distance in pixels out from the tip of the gun in which souls can be sucked in.
        private const float Size = 480f;
        // The spread of the suction on each side. 25 degrees on each side, 50 degrees total.
        private const float Spread = MathHelper.Pi / 7.2f;
        // Manual offset used for drawing the gun.
        private static Vector2 Offset => new Vector2(27f, -10f);
        // The position of the tip of the gun. Used for most projectile logic.
        public Vector2 TipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * 62f + Vector2.UnitY * Offset.Y;
        // A list containing the ai[0] values of each soul that is sucked in, used for determining the color to draw the soul as.
        public List<float> SoulColors = [];
        // Tracks whether or not the gun should be doing the laser attack.
        private bool FiringLasers = false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Stop using the holdout if you cannot
            if (Owner.CantUseHoldout(false))
            {
                Projectile.Kill();
                return;
            }
            Timer++;

            // If you release right click, use the laser attack
            if (!Owner.Calamity().mouseRight)
                FiringLasers = true;
            if (FiringLasers)
            {
                if (SoulColors.Count > 0)
                {
                    if (Timer % 10 == 0f)
                    {
                        if (Owner.HeldItem.ModItem is SHPC shpc && shpc.storedSoulpower > 0)
                        {
                            shpc.storedSoulpower -= Math.Min(shpc.storedSoulpower, 2);
                            SoundEngine.PlaySound(CommonCalamitySounds.ELRFireSound, Owner.Center);
                            Vector2 laserVel = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld) * 20f;
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), TipPosition, laserVel, ModContent.ProjectileType<SHPL>(), Projectile.damage, 3f, Projectile.owner, SoulColors[0]);
                            SoulColors.RemoveAt(0);
                        }
                        else
                            SoulColors.Clear();
                    }
                }
            }
            else
            {
                // Debug code used for assessing the visual and functional area of the vacuum
                /*BloomLineVFX l = new(Owner.Center, Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).RotatedBy(Spread) * Size, 0.4f, Color.Gray, 2, true);
                BloomLineVFX l2 = new(Owner.Center, Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).RotatedBy(-Spread) * Size, 0.4f, Color.Gray, 2, true);
                GeneralParticleHandler.SpawnParticle(l);
                GeneralParticleHandler.SpawnParticle(l2);*/

                // Spawn faded smoke particles idly
                if (Timer % 2 == 0f)
                {
                    Vector2 spawn = Owner.Center + Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).RotatedByRandom(Spread) * Size * Main.rand.NextFloat(0.4f, 1f);
                    MediumMistParticle smoky = new(spawn, Utils.DirectionTo(spawn, TipPosition) * 4.5f, Color.Gray, Color.DarkGray, 1.25f, 96f);
                    GeneralParticleHandler.SpawnParticle(smoky);
                }
            }

            // Typical held projectile shit
            Owner.heldProj = Projectile.whoAmI;
            Owner.ChangeDir(Math.Sign((Owner.Calamity().mouseWorld - Owner.Center).X));
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Owner.Center - Owner.Calamity().mouseWorld).ToRotation() * Owner.gravDir + MathHelper.PiOver2);
            Projectile.rotation = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld).ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = Owner.Center;
            // This is used to make the projectile naturally despawn after running out of souls
            if (!FiringLasers || SoulColors.Count > 0)
                Projectile.timeLeft = 30;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool withinAngle = Math.Abs(Utils.DirectionTo(TipPosition, Owner.Calamity().mouseWorld).ToRotation() - Utils.DirectionTo(TipPosition, targetHitbox.Center.ToVector2()).ToRotation()) <= Spread;
            // This extra safety hitbox is a square around the gun tip used so that the soul doesn't need to perfectly travel down the narrow end of the vacuum to get sucked in
            Rectangle extraSafetyHitbox = new Rectangle((int)TipPosition.X - (Projectile.width / 2), (int)TipPosition.Y - (Projectile.height / 2), Projectile.width, Projectile.height);
            return (CalamityUtils.CircularHitboxCollision(TipPosition, Size, targetHitbox) && withinAngle) || targetHitbox.Intersects(extraSafetyHitbox);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw spiraling smoke particles representing the suction radius
            Vector2 farthestPos = Owner.Center + Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld) * Size;
            float rotation = Utils.DirectionTo(TipPosition, farthestPos).ToRotation();
            if (!FiringLasers)
            {
                Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/MediumMist").Value;
                Rectangle frame = tex.Frame(1, 3, 0, Main.rand.Next(3));

                Main.spriteBatch.SetBlendState(BlendState.Additive);
                for (int i = 0; i < 6; i++)
                {
                    // There are 3 separate streams offset from one another
                    for (int j = 0; j < 3; j++)
                    {
                        float distRatio = 1f - (Main.GameUpdateCount + j * 10) % 30 / 30f;
                        Vector2 posOffset = new Vector2(MathF.Sin(MathHelper.TwoPi / 6f * i) * 25f * distRatio, MathF.Cos(Main.GameUpdateCount * MathHelper.Pi / 30f + MathHelper.TwoPi / 6f * i) * 160f * distRatio).RotatedBy(rotation);
                        float colorMult = 0.5f * Utils.GetLerpValue(1f, 0.8f, distRatio, true);
                        Main.EntitySpriteDraw(tex, Vector2.Lerp(TipPosition, farthestPos, distRatio) + posOffset - Main.screenPosition, frame, Color.Gray * colorMult, rotation + MathHelper.Pi, frame.Size() / 2f, 1.5f * distRatio, SpriteEffects.None);
                    }
                }
                Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            }

            // Manually draw the gun itself
            Texture2D shpc = TextureAssets.Item[ModContent.ItemType<SHPC>()].Value;
            Vector2 position = Owner.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rotation) * Offset.X + Vector2.UnitY * Offset.Y;
            SpriteEffects sp = Owner.Calamity().mouseWorld.X < Owner.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(shpc, position, null, lightColor, rotation, shpc.Size() / 2f, 1f, sp);
            return false;
        }
    }
}
