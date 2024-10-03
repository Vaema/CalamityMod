using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.ExtraTextures;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class DivineHatchetBoomerang : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/SeekingScorcher";

        // Stats
        public const int ReboundTime = 100;
        public const int MaxBounces = 3;
        public const float MaxHomingRange = 1000f; // 62.5 blocks

        private List<int> PreviousNPCs = new List<int>() { -1 };
        public Player Owner => Main.player[Projectile.owner];
        public ref float AirTime => ref Projectile.ai[0];

        public Particle Smear;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.MaxUpdates = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150 * Projectile.MaxUpdates;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (AirTime % 8f == 0f)
                SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);

            // Boomerang rotation
            Projectile.rotation += Projectile.direction * 0.4f;

            if (Smear == null)
            {
                Smear = new CircularSmearVFX(Projectile.Center, Color.Black, Projectile.rotation, 0.63f, ExtraTextureRefs.CircularSmearFire3);
                GeneralParticleHandler.SpawnParticle(Smear);
            }
            else
            {
                Smear.Rotation = Projectile.rotation + MathHelper.ToRadians(75f);
                Smear.Time = 0;
                Smear.Position = Projectile.Center;
                Smear.Scale = 0.63f;
                Smear.Color = Color.Gold;
            }

            //holy dust
            if (Main.rand.NextBool(8))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.CopperCoin, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }

            // Returns after some number of frames in the air
            AirTime++;
            if (AirTime > ReboundTime)
                ReturnToOwner();
            else if (AirTime == ReboundTime)
                SeekNPC();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        // Glowmask
        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 200);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 180);

            // Disallow the NPC to be targeted again
            PreviousNPCs.Add(target.whoAmI);
            if (AirTime <= ReboundTime)
                SeekNPC();
        }

        public void SeekNPC()
        {
            // Return if exceeding max number of bounces
            if (Projectile.numHits >= MaxBounces)
            {
                AirTime = ReboundTime + 1f;
                ReturnToOwner();
                return;
            }

            // Find the closest NPC targetable
            float range = MaxHomingRange;
            int targetNPC = -1;
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!target.CanBeChasedBy(Projectile) || PreviousNPCs.Contains(target.whoAmI))
                    continue;

                float distance = Vector2.Distance(target.Center, Projectile.Center);
                if (distance < range && Collision.CanHit(Projectile, target))
                {
                    range = distance;
                    targetNPC = target.whoAmI;
                }
            }

            // Move towards the target if found and slightly restore airtime
            if (targetNPC != -1f)
            {
                AirTime -= 6f;
                Projectile.velocity = Projectile.SafeDirectionTo(Main.npc[targetNPC].Center) * 15f;
            }
        }

        public void ReturnToOwner()
        {
            // Swiftly move back towards the player
            Projectile.velocity = Projectile.SafeDirectionTo(Owner.Center) * 20f;

            // Delete the projectile if it touches its owner or too far away.
            if (Projectile.Hitbox.Intersects(Owner.Hitbox) || Vector2.Distance(Projectile.Center, Owner.Center) >= 3000f)
                Projectile.Kill();
        }
    }
}
