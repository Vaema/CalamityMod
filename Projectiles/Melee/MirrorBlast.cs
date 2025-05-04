using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CalamityMod.Projectiles.Melee
{
    public class MirrorBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public bool isShard = true;

        public int shardShield = 0;

        public bool isShield => shardShield > 0;

        private bool hasSpawned = false;

        public int shardNum = -1;

        private Player player => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1800;
            Projectile.extraUpdates = 0;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage()
        {
            return !isShard;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.Calamity().DR > 0.9f) {
                return false;
            }
            return base.CanHitNPC(target);
        }
        public override void AI()
        {
            if (!hasSpawned)
            {
                shardNum = player.ownedProjectileCounts[Projectile.type];
                hasSpawned = true;
                shardNum = 0;
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<MirrorBlast>() && proj.owner == Projectile.owner)
                    {
                        (proj.ModProjectile as MirrorBlast).shardNum++;
                    }
                }
            }
            if (isShield)
            {
                if (shardNum > 10)
                {
                    shardShield = 0;
                    isShard = false;
                    Projectile.timeLeft = 1200;
                    Projectile.velocity = Projectile.DirectionTo(player.Center) * -20f;
                    return;
                }
                List<Vector2> positions = new List<Vector2>()
                {
                    new(0,75),
                    new(10,65),
                    new(-10,65),
                    new(20,75),
                    new(-20,75),
                    new(30,65),
                    new(-30,65),
                    new(14,55),
                    new(-14,55),
                    new(0,45)
                };
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = Vector2.Lerp(Projectile.Center,
                player.Center + player.DirectionTo(player.Calamity().mouseWorld)
                    .RotatedBy(MathHelper.ToRadians(positions[shardNum - 1].X))
                    * positions[shardNum - 1].Y,
                0.5f);
                Projectile.rotation = Projectile.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
                shardShield--;
            }
            else if (isShard)
            {
                Projectile.velocity = Vector2.Zero;
                var shardCount = 0;
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<MirrorBlast>() && proj.owner == Projectile.owner && (proj.ModProjectile as MirrorBlast).isShard)
                    {
                        shardCount++;
                    }
                }
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + new Vector2(0, MathHelper.Lerp(-90, -110, MathF.Sin(player.miscCounter / 300f * MathHelper.TwoPi + MathHelper.Pi * (shardNum % 2)))).RotatedBy(MathHelper.ToRadians(player.miscCounter / 300f * 360f + 360f / shardCount * shardNum)), 0.15f);
                Projectile.rotation = Projectile.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;
                if (shardNum > 10 || Projectile.timeLeft < 2)
                {
                    isShard = false;
                    Projectile.timeLeft = 1200;
                    Projectile.velocity = Projectile.DirectionTo(player.Center) * -20f;
                }
            }
            else
            {
                float homingStrength = 0.025f; // Adjust this value for stronger or weaker homing
                NPC target = FindClosestNPC(3200f);
                if (target != null)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= 40f; // Adjust speed as needed
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, homingStrength);
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            Lighting.AddLight(Projectile.Center, 0.96f*0.33f, 0.91f*0.33f, 0.33f);
            if (Projectile.FinalExtraUpdate())
                Projectile.frameCounter++;
            if (Projectile.frameCounter > 8)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 3)
                Projectile.frame = 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D BlastTex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/MirrorBlast").Value;
            Texture2D ShardTex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/MirrorShard").Value;
            Point BlastTextureDim = new Point(60, 26);
            Point ShardTextureDim = new Point(32, 14);
            Texture2D UsedTex = isShard ? ShardTex : BlastTex;
            Point UsedTextureDim = isShard ? ShardTextureDim : BlastTextureDim;
            Vector2 origin = isShard ? ShardTextureDim.ToVector2() / 2f : BlastTextureDim.ToVector2() / 2f + new Vector2(14, 0);
            Main.spriteBatch.Draw(UsedTex, Projectile.Center - Main.screenPosition, new Rectangle(0, UsedTextureDim.Y * Projectile.frame, UsedTextureDim.X, UsedTextureDim.Y), Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closestNPC = null;
            float closestDistance = maxRange;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(this) && !npc.friendly)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            return closestNPC;
        }
    }
}
