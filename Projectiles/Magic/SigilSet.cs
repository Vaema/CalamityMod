using System;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SigilSet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public ref float FadeoutFlag => ref Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 550;
            Projectile.height = 550;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Culling Logic
            Player p = Main.player[Projectile.owner];
            if (p == null || !p.active || p.dead || p.HeldItem.type != ModContent.ItemType<UnstableCastersGauntlet>())
            {
                Projectile.Kill();
                return;
            }

            // Visuals
            Projectile.Center = p.MountedCenter + Vector2.UnitY * p.gfxOffY;
            Projectile.rotation += 0.01f;
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);

            if (Projectile.frameCounter++ > 3)
            {
                Projectile.frameCounter = 0;
                if (Projectile.frame++ > 2)
                {
                    Projectile.frame = 0;
                }
            }

            // Fading logic
            if (FadeoutFlag == 1f)
            {
                Projectile.alpha += 13;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.alpha = Utils.Clamp(Projectile.alpha - 25, 0, 255);
            }


            if (Projectile.ai[1] == 0) // spawn flag
            {
                int[] sigilTypes = new int[]
                {
                    ModContent.ProjectileType<IgnisSigil>(),
                    ModContent.ProjectileType<AquaSigil>(),
                    ModContent.ProjectileType<TerraSigil>(),
                    ModContent.ProjectileType<AerSigil>(),
                    ModContent.ProjectileType<OrdoSigil>(),
                    ModContent.ProjectileType<PerditoSigil>()
                };
                for (int i = 0; i < 6; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, sigilTypes[i], Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.identity, i);
                }

                // Make it not happen again
                Projectile.ai[1] = 1;
            }
            else // Sigils exist
            {
                int activeSigilCount = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.ai[0] == Projectile.identity)
                    {
                        // Check if the sigil is not currently fading out
                        if (proj.type == ModContent.ProjectileType<IgnisSigil>() ||
                            proj.type == ModContent.ProjectileType<AquaSigil>() ||
                            proj.type == ModContent.ProjectileType<TerraSigil>() ||
                            proj.type == ModContent.ProjectileType<AerSigil>() ||
                            proj.type == ModContent.ProjectileType<OrdoSigil>() ||
                            proj.type == ModContent.ProjectileType<PerditoSigil>())
                        {
                            if (proj.ai[2] <= 0)
                            {
                                activeSigilCount++;
                            }
                        }
                    }
                }

                // If no non-fading sigils remain, start timer
                if (activeSigilCount == 0)
                {
                    Projectile.localAI[0]++;
                    if (Projectile.localAI[0] >= 50)
                    {
                        FadeoutFlag = 1f;
                    }
                }
                else
                {
                    Projectile.localAI[0] = 0;
                }
            }
            Projectile.timeLeft = 4;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D main = TextureAssets.Projectile[Type].Value;
            Texture2D smol = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/ThaumRingSmall").Value;
            Texture2D rune = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/ThaumRune").Value;

            // Modify opacity again so it doesn't override changes to alpha in AI
            float drawOpacity = 1f - Projectile.alpha / 255f;

            // Ring components
            Main.EntitySpriteDraw(smol, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f * drawOpacity, Projectile.rotation, smol.Size() / 2, Projectile.scale + MathF.Cos(2 * Main.GlobalTimeWrappedHourly + 5) * 0.0027f, 0);
            Main.EntitySpriteDraw(main, Projectile.Center - Main.screenPosition, main.Frame(1, 4, 0, Projectile.frame), Color.White * 0.5f * drawOpacity, Projectile.rotation, new Vector2(main.Width / 2, main.Height / 8), Projectile.scale, 0);
            for (int i = 0; i < 14; i++)
            {
                float dist = 150f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 10;
                Vector2 runePos = Projectile.Center + (Vector2.UnitX.RotatedBy(MathHelper.Lerp(0, MathHelper.TwoPi, i / 14f)) * dist).RotatedBy(-Projectile.rotation * 0.6f);
                Main.EntitySpriteDraw(rune, runePos - Main.screenPosition, rune.Frame(1, 7, 0, i % 6), Color.White * 0.5f * drawOpacity, runePos.DirectionTo(Projectile.Center).ToRotation() + MathHelper.PiOver2, new Vector2(rune.Width / 2, rune.Height / 14), Projectile.scale, 0);
            }

            return false;
        }

        public override bool? CanDamage() => false;
    }
}
