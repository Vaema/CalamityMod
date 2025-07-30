using CalamityMod.Balancing;
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Particles;
using CalamityMod.CalPlayer;

namespace CalamityMod.Projectiles.Typeless
{
    public class StratusBlackHole : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless"; 
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 166;
            Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 6000 * Projectile.MaxUpdates;
            Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Main.projFrames[Type] = 4;
            Projectile.velocity *= 0.975f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 10)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;
            if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity = Projectile.timeLeft / 30f;
            }
            if (Projectile.velocity.Length() < 1 && Projectile.timeLeft % 30 == 0 && Main.LocalPlayer.whoAmI == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(15 + 10 * Main.rand.NextFloat(), 0).RotatedByRandom(MathHelper.TwoPi), ModContent.ProjectileType<StratusHawkingRadiation>(), (int)(Projectile.damage * 0.25f), 1, Projectile.owner);
            }
            Projectile.rotation = MathHelper.Lerp(0, Projectile.velocity.SafeNormalize(Vector2.Zero).X, Projectile.velocity.Length() / 20f);
            if (Projectile.frameCounter == 0 && Projectile.frame == 0)
            {
                GeneralParticleHandler.SpawnParticle(new CustomSpark(Projectile.Center + new Vector2(Main.rand.Next(16,24)).RotatedByRandom(MathHelper.TwoPi), Vector2.Zero, "CalamityMod/Particles/Sparkle",false,30,0.75f,Color.SkyBlue,Vector2.One));
            }
        }

         public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[Projectile.owner];
            player.Calamity().StratusStarburst++;
            if (player.Calamity().StratusStarburst <= CalamityPlayer.MaxStratusStarburst)
                player.Calamity().StarburstEntities.Add(new DataStructures.StarburstEntity(Projectile.Center));
            player.Calamity().HasStratusItemCooldown = (int)MathHelper.Max(player.Calamity().HasStratusItemCooldown, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.Lerp(Color.White,Color.SkyBlue, 0.75f);
            return base.PreDraw(ref lightColor);
        }
    }
}
