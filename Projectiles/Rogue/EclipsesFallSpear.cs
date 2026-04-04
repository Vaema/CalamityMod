using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EclipsesFallSpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/EclipsesFall";
        private int SplitProjDamage => (int)(Projectile.damage * EclipsesFall.FragmentDmgMult);

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.MaxUpdates = 2;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6 * Projectile.MaxUpdates;
            Projectile.timeLeft = 150 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            if (Main.rand.NextBool(5))
            {
                Vector2 trailPos = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(-16f, 16f);
                float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
                Color trailColor = Main.rand.NextBool() ? Color.Indigo : Color.DarkOrange;
                Particle eclipseTrail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(eclipseTrail);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int onHitCount = EclipsesFall.FragmentCount + 4;
            float spread = 20f;
            int projectileDamage = SplitProjDamage;
            float kb = 5f;
            int sparkID = ModContent.ProjectileType<EclipseSpark>();
            int starID = ModContent.ProjectileType<EclipseFragment>();
            for (int i = 0; i < onHitCount; i++)
            {
                int projID = i < EclipsesFall.FragmentCount ? starID : sparkID;
                Vector2 velocity = Projectile.oldVelocity.RotateRandom(MathHelper.ToRadians(spread)) * 0.5f;
                float speed = Main.rand.NextFloat(1.5f, 2f);
                float moveDuration = Main.rand.Next(5, 15);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity * speed, projID, projectileDamage, kb, Projectile.owner, 0f, moveDuration, 20);
            }

            SoundEngine.PlaySound(SoundID.Item62 with { Volume = SoundID.Item62.Volume * 0.6f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item68 with { Volume = SoundID.Item68.Volume * 0.2f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = SoundID.Item122.Volume * 0.4f }, Projectile.position);


            List<Projectile> frags = new();
            var p = -2;
            foreach (var item in Main.ActiveProjectiles)
            {

                if (item.type == ModContent.ProjectileType<EclipseSpear>() && item.owner == Projectile.owner && item.Opacity > 0.1f)
                {
                    if (Projectile.Calamity().stealthStrike && item.timeLeft > 600 * item.MaxUpdates)
                    {
                        if (item.timeLeft < 1120 * item.MaxUpdates)
                        {
                            item.timeLeft = 60 * item.MaxUpdates;
                            item.ai[0] = target.whoAmI;
                            continue;
                        }
                        item.timeLeft = 1200 * item.MaxUpdates;
                        p = item.whoAmI;
                    }
                    item.ai[0] = target.whoAmI;
                }
                if (item.type == ModContent.ProjectileType<EclipseFragment>() && item.owner == Projectile.owner && item.ai[0] > -2)
                {
                    frags.Add(item);
                }
            }
            if (Projectile.Calamity().stealthStrike && p < 0)
                for (int i = 0; i < 1; i++)
                {
                    int projID = ModContent.ProjectileType<EclipseSpear>();
                    Vector2 velocity = Vector2.Zero;
                    var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.SafeNormalize(default) * 64f, projID, Projectile.damage, kb, Projectile.owner, target.whoAmI, 0, Math.Min(20, frags.Count(x => x.ai[0] == 0)));
                    proj.rotation = Projectile.rotation;
                    p = proj.whoAmI;
                }


            frags = frags.OrderBy(x => x.timeLeft).ToList();
            int toRemove = frags.Count(x => x.ai[0] == 0) - EclipsesFall.MaxFragmentCount;
            foreach (var item in frags)
            {
                if (toRemove > 0 && item.ai[0] == 0)
                {
                    item.ai[0] = -2;
                    item.timeLeft = EclipseFragment.lifetime;
                    toRemove--;
                    continue;
                }
                if (Projectile.Calamity().stealthStrike)
                {
                    if (item.ai[0] == 0)
                    {
                        item.ai[1] = Main.rand.Next(5, 15);
                        item.ai[2] = 20;
                    }
                    item.ai[0] = p + 1;
                    item.timeLeft = EclipseFragment.lifetime;
                    item.damage = 0;
                    item.netUpdate = true;
                }
            }


        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>(Texture + "Glow").Value;
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
        }
    }
}
