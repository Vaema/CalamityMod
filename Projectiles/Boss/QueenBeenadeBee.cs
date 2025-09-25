using System;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class QueenBeenadeBee : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GiantBee}";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
        }

        public override void AI()
        {
            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 3)
                Projectile.frame = 0;

            // Direction and rotation
            Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;
            Projectile.rotation = Projectile.velocity.X * 0.1f;

            // Difficulty modes
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Get a target and calculate distance from it
            int target = Player.FindClosest(Projectile.Center, 1, 1);
            Vector2 distanceFromTarget = Main.player[target].Center - Projectile.Center;

            // Set AI to stop homing, start accelerating
            float stopHomingDistance = 120f;
            if (distanceFromTarget.Length() < stopHomingDistance || Projectile.ai[0] == 1f || Projectile.timeLeft < 120)
            {
                Projectile.ai[0] = 1f;

                Projectile.tileCollide = true;

                if (Projectile.velocity.Length() < Projectile.ai[2])
                    Projectile.velocity *= 1.05f;

                return;
            }

            // Fly off for a bit before homing
            if (Projectile.timeLeft > 300)
            {
                if (Projectile.velocity.Length() < Projectile.ai[1])
                    Projectile.velocity *= 1.05f;

                return;
            }

            // Home in on target
            float scaleFactor = Projectile.velocity.Length();
            float inertia = 16f;
            distanceFromTarget.Normalize();
            distanceFromTarget *= scaleFactor;
            Projectile.velocity = (Projectile.velocity * inertia + distanceFromTarget) / (inertia + 1f);
            Projectile.velocity.Normalize();
            Projectile.velocity *= scaleFactor;

            // Fly away from other bees
            float pushForce = 0.05f;
            float pushDistance = 60f;
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile otherProj = Main.projectile[k];
                // Short circuits to make the loop as fast as possible
                if (!otherProj.active || k == Projectile.whoAmI)
                    continue;

                // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                bool sameProjType = otherProj.type == Projectile.type;
                float taxicabDist = Vector2.Distance(Projectile.Center, otherProj.Center);
                if (sameProjType && taxicabDist < pushDistance)
                {
                    if (Projectile.position.X < otherProj.position.X)
                        Projectile.velocity.X -= pushForce;
                    else
                        Projectile.velocity.X += pushForce;

                    if (Projectile.position.Y < otherProj.position.Y)
                        Projectile.velocity.Y -= pushForce;
                    else
                        Projectile.velocity.Y += pushForce;
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(BuffID.Poisoned, 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 150, Projectile.velocity.X, Projectile.velocity.Y, 50);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = 1f;
            }
        }
    }
}
