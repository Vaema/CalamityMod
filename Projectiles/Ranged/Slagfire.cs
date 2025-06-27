using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class Slagfire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {

            Projectile.localAI[1]++;
            if (Projectile.localAI[1] >= 4f)
            {
                Projectile.tileCollide = true;
            }
            Projectile.scale -= 0.001f;
            if (Projectile.scale <= 0f)
            {
                Projectile.Kill();
            }
            if (Projectile.localAI[0] <= 3f)
            {
                Projectile.localAI[0] += 1f;
                return;
            }


            Color trailColor = Color.PaleVioletRed;

            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkPosition = Projectile.Center - (Projectile.velocity / 3f * i);

                Vector2 sparkVelocity = -Projectile.velocity * 0.01f * Main.rand.NextFloat(0.5f, 1.5f);

                Particle spark = new CustomSpark(
                  sparkPosition,
                  sparkVelocity,
                  "CalamityMod/Particles/BloomLineFade",
                  false,
                  6,
                  0.04f,
                  trailColor * 0.85f,
                  new Vector2(0.8f, 1), // Scaling for the end
                  shrinkSpeed: 0.4f);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPosition = Projectile.Center + Main.rand.NextVector2Circular(15, 15);
                Vector2 sparkVelocity = -Projectile.velocity * Main.rand.NextFloat(0.01f, 0.045f); // Backward velocity

                Particle subtleSpark = new CustomSpark(
                sparkPosition,
                sparkVelocity,
                "CalamityMod/Particles/BloomLineFade",
                false,
                8,
                0.02f,
                trailColor * 0.5f,
                new Vector2(0.5f, 0.8f),
                shrinkSpeed: 0.3f);
                GeneralParticleHandler.SpawnParticle(subtleSpark);
            }

            if (Projectile.localAI[1] >= 10f)
                Projectile.velocity.Y += 0.2f;

        }

        public override bool CanHitPlayer(Player target)
        {
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.type == NPCID.Guide)
            {
                modifiers.FinalDamage *= 10f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damage)
        {
            if (target.type == NPCID.Guide)
            {

                if (target.life <= 0)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDustPerfect(target.Center, (int)CalamityDusts.Brimstone,
                        new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-4f, 4f)),
                        150, default, 2f).noGravity = true;
                    }

                    if (!NPC.AnyNPCs(NPCID.WallofFlesh)) // Don't spawn if Wall is already active
                    {
                        int playerIndex = Projectile.owner;
                        if (playerIndex >= 0 && Main.player[playerIndex].active)
                        {
                            NPC.SpawnOnPlayer(playerIndex, NPCID.WallofFlesh); // The game handles spawning logic by itself from here
                        }
                    }
                }
            }
        }
    }
}
