using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Chat;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;

namespace CalamityMod.Projectiles.Ranged
{
    public class GaleforceArrow : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public PrimitiveTrail TrailDrawer = null;
        public Vector2[] TrailPositions = new Vector2[10];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        public override void AI()
        {
            Vector2 destination = Projectile.Center;
            bool locatedTarget = false;

            // Find a target.
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                float extraDistance = (Main.npc[i].width / 2) + (Main.npc[i].height / 2);
                if (!Main.npc[i].CanBeChasedBy(Projectile, false) || !Projectile.WithinRange(Main.npc[i].Center, 340f + extraDistance))
                    continue;

                //Adds two normalized vectors together to determine how close the angle is between them
                Vector2 coneHomeCheck = (Main.npc[i].Center - Projectile.Center).SafeNormalize(Vector2.Zero) + Projectile.velocity.SafeNormalize(Vector2.Zero);
                if (Collision.CanHit(Projectile.Center, 1, 1, Main.npc[i].Center, 1, 1) && (Math.Abs(coneHomeCheck.X) > 1.15f || Math.Abs(coneHomeCheck.Y) > 1.15f)) 
                {
                    destination = Main.npc[i].Center;
                    locatedTarget = true;
                    break;
                }
            }

            if (locatedTarget)
            {
                // Home in on the target.
                Vector2 homeDirection = (destination - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * 12f + homeDirection * 20f) / (12f + 1f);
            }

            if (Projectile.velocity.Length() < 20f) // NO SLOWING DOWN
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 20f;
            }

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 133, Scale: 0.4f);
                dust.noGravity = true;
            }   
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY) * 10f, ModContent.ProjectileType<GaleforceWind>(), (int)(Projectile.damage / 2f), Projectile.knockBack, Projectile.owner, target.whoAmI);
        }
        public Color TrailColor(float completionRatio)
        {
            Color color = Color.Lerp(Color.Cyan, Color.LightCyan, completionRatio);
            color = Color.Lerp(color, Color.White, completionRatio);
            color.A = (byte)(int)(Utils.GetLerpValue(0.7f, 0f, completionRatio) * 255);
            return color;
        }

        public float TrailWidth(float completionRatio)
        {
            return MathHelper.Lerp(5f, 0f, completionRatio);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (TrailDrawer is null)
                TrailDrawer = new PrimitiveTrail(TrailWidth, TrailColor, null, GameShaders.Misc["CalamityMod:GaleforceArrowTrail"]);

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            //regular draw position BUT the start point is displaced an extra amount towards the tip of the arrow. The final float is to further multiply this displacement
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Projectile.height * 2.2f);
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = -1; i <= 1; i += 2)
            {
                for (int oldPositions = 0; oldPositions < 10; oldPositions++)
                {

                    if (Projectile.oldPos[oldPositions] != Vector2.Zero) //remove positions that are invalid due to the projectile not being alive long eonugh
                    {
                        //Building a better line of trail positions rather than just simply using oldPos. Angled to the side slightly with a log to smooth it out. i is the direction the trail will go, as this is 2 trails rendered at once. the final float is to further exxagerate the gap between the trails.
                        TrailPositions[oldPositions] = Projectile.oldPos[oldPositions] + ((Projectile.oldRot[oldPositions] + MathHelper.PiOver2 + (MathHelper.Pi/2.5f * i)).ToRotationVector2() * (float)Math.Log(2 * oldPositions + 1, 1.36d) * 2.5f);
                    }
                }
                Main.spriteBatch.EnterShaderRegion();
                GameShaders.Misc["CalamityMod:GaleforceArrowTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                GameShaders.Misc["CalamityMod:GaleforceArrowTrail"].Apply();

                TrailDrawer.Draw(TrailPositions, Projectile.Size * 0.5f - Main.screenPosition, 6);
                Main.spriteBatch.ExitShaderRegion();

                Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, 0, 0);
            }
            
            return false;
        }
    }
}
