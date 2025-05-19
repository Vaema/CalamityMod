using Terraria.ModLoader;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using CalamityMod.Items.Weapons.Melee;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Melee
{

    public class MirrorBladeProjectile : BaseSwordHoldoutProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override int swingWidth => 200;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<MirrorBlade>()).Item;
        public override int AfterImageLength => 10;
        public override int OffsetDistance => 80;
        public override bool drawSwordTrail => false;
        public override Color[] trailColors => new Color[] { Color.Red, Color.MediumPurple, Color.Purple }; public override int StartupTime { get; set; }
        public override int CooldownTime { get; set; }
        public override bool AlternateSwings => true;

        public override bool useMeleeSpeed => true;

        public override int swingTime { get; set; } = 8;

        public override SoundStyle? UseSound => SoundID.Item1 with {Volume = 0.7f};

        public List<int> reflectedProjectiles = new List<int>() { };

        public override void Defaults()
        {
            Projectile.extraUpdates = 3;
        }

        public override void Spawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
            StartupTime = 15;
            CooldownTime = 15;
            swingTime -= StartupTime + CooldownTime;
            modplayer.swingNum = (modplayer.swingNum + 1) % 2;
            Projectile.timeLeft = 600;
            
        }

        public override void AdditionalAI()
        {
            if (inStartup)
            {
                Projectile.scale = baseScale * MathHelper.Lerp(0.75f, 1f, StartupCompletion);
            }
            else if (inCooldown)
            {
                Projectile.Opacity = 1;
                Projectile.scale = baseScale * MathHelper.Lerp(1, 0.75f, CooldownCompletion);
            }
            else
            {
                Projectile.Opacity = 1;
            }
            if (!inStartup && !inCooldown)
                foreach (var proj in Main.ActiveProjectiles)
                {
                    if (proj.Hitbox.Intersects(Projectile.Hitbox) && proj.hostile && !reflectedProjectiles.Contains(proj.whoAmI))
                    {
                        reflectedProjectiles.Add(proj.whoAmI);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), proj.Center, proj.velocity * -1, ModContent.ProjectileType<MirrorBlast>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact,Projectile.Center);
                    }
                }
                
            Lighting.AddLight(Main.player[Projectile.owner].Center, 0.96f, 0.91f, 1f);
        }

        public override float SwingFunction()
        {
            if (inStartup)
                return MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth * 0.7f, -swingWidth * 0.4f, 1 - MathF.Pow(StartupCompletion, 0.5f)));
            if (inCooldown)
                return MathHelper.ToRadians(MathHelper.SmoothStep(swingWidth * 0.5f, (360 - swingWidth * 0.4f), MathF.Pow(CooldownCompletion, 0.5f)));
            return MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth * .5f, (swingWidth * 0.5f), SwingCompletion));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (var i = 0; i < 2; i++)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<MirrorBlast>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact,Projectile.Center);
        }
        public override float trailOffset => 28;
        public override float trailWidth(float completion)
        {
            return 60;
        }
        public override int trailLength => 40;
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            return base.PreDraw(ref lightColor);
        }
    }
}