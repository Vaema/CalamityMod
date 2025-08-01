using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Shortswords
{
    public class GalileoGladiusProj : BaseSwordHoldoutProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<GalileoGladius>();
        public Player Owner => Main.player[Projectile.owner];
        public override int swingWidth => 200;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<GalileoGladius>()).Item;
        public override string Texture => BaseItem.ModItem.Texture;
        public override int AfterImageLength => 0;
        public override int OffsetDistance { get; set; } = 90;
        public override bool drawSwordTrail => false;
        public override bool AlternateSwings => false;

        public override bool useMeleeSpeed => true;

        public override int swingTime { get; set; } = 8;

        public override SoundStyle? UseSound => SoundID.Item71 with { Volume = 0.5f };

        public override void Defaults()
        {
            Projectile.extraUpdates = 3;
        }

        public override void Spawn(IEntitySource source)
        {
            angle = angle.RotatedByRandom(0.2f);
        }

        public override void AdditionalAI()
        {
            OffsetDistance = (int)MathHelper.Lerp(15, 45, SwingCompletion);
            //Spawn the large glow V
            var sparkAngle = angle.RotatedBy(MathHelper.Pi);
            int dir = MathF.Sign(sparkAngle.X);
            var color = Color.LightSkyBlue;
            if (Projectile.FinalExtraUpdate())
            {
                for (float i = -1; i <= 1; i += 2f)
                {
                    Vector2 velocity = -sparkAngle.RotatedBy(i * -0.3f) * 10f;
                    Vector2 position = Projectile.Center + new Vector2(MathHelper.Lerp(20, 100, SwingCompletion), i * 15).RotatedBy(sparkAngle.ToRotation());
                    Particle spark = new CustomSpark(position, velocity, "CalamityMod/Particles/BloomCircle", false, 3, 0.3f, color, new Vector2(0.3f, 3f),noShrink:true);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Projectile.FinalExtraUpdate())
                    GeneralParticleHandler.SpawnParticle(new CustomSpark(Projectile.Center + sparkAngle * MathHelper.Lerp(0, 60, SwingCompletion), sparkAngle, "CalamityMod/Particles/GlowBlade", false, 3, 0.04f, Color.SkyBlue, new Vector2(MathHelper.Lerp(0.5f, 1f, SwingCompletion), MathHelper.Lerp(0.1f, 1.5f, SwingCompletion)), shrinkSpeed: 0.2f));
            }
            Lighting.AddLight(Main.player[Projectile.owner].Center, 0.96f, 0.91f, 1f);
        }

        public override float SwingFunction()
        {
            return 0; //Galileo stabs, not swings.
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Voidfrost>(), 300);
            Owner.Calamity().StratusStarburst++;
        }

        //This can be deleted once Gilded Proboscis is merged into the main branch.
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            var lineCollisionLength = 160;
            var player = Main.player[Projectile.owner];
            var armcenter = player.Center - new Vector2(5 * player.direction, 2);
            var swordDir = armcenter.DirectionTo(Projectile.Center);
            var collisionline = new Vector2(lineCollisionLength / 2f, 0).RotatedBy(swordDir.ToRotation()) * Projectile.scale;
            bool c = Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, Projectile.Center + collisionline);
            if (c && !float.IsNaN(collisionline.X) && !float.IsNaN(collisionline.Y))
                return true;
            return base.Colliding(projHitbox, targetHitbox);
        }

    }
}
