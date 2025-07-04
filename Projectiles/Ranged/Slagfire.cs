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
            Projectile.tileCollide = true;
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


            Color trailColor = new Color(214, 51, 70);

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

            // Falling particles to appear as lava droplets, similar to speed blaster's falling dust
            if (Main.rand.NextBool(50))
            {

                Vector2 spawnHere = Projectile.position;
                spawnHere.Y = Projectile.Bottom.Y + 3f; // A little down

                Color coreSlagColor = new Color(247, 111, 77);
                Color outerSlagColor = new Color(242, 157, 170);
                Vector2 particleVelocity = new Vector2(Main.rand.Next(-1, 1), 3);

                Color particleColor = Color.Lerp(coreSlagColor, outerSlagColor, Main.rand.NextFloat());

                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(spawnHere, particleVelocity, true, // Affected by gravity
                Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.65f), particleColor));
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

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Color coreSlagColor = new Color(247, 111, 77);
            Color outerSlagColor = new Color(242, 157, 170);

            Vector2 collisionPoint = Projectile.position;


            //These adjustments are made to look more embedded into tiles instead of them seemingly floating above where they landed
            if (oldVelocity.Y != 0f) // Hitting a horizontal surface
            {
                if (oldVelocity.Y > 0f)
                    collisionPoint.Y = Projectile.Bottom.Y + 8f; // Spawn inside bottom edge
                else
                    collisionPoint.Y = Projectile.Top.Y - 8f; // Spawn inside top edge. Yes, this should also be negative.
            }
            if (oldVelocity.X != 0f) // Hitting a vertical surface
            {
                if (oldVelocity.X > 0f)
                    collisionPoint.X = Projectile.Right.X + 2.5f; // Spawn slightly inside right edge of projectile
                else
                    collisionPoint.X = Projectile.Left.X - 2.5f; // Spawn slightly inside left edge of projectile
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnPosition = collisionPoint + Main.rand.NextVector2Circular(2f, 2f); 

                Vector2 particleVelocity = oldVelocity.SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(0.5f, 1.5f);

                particleVelocity.Y += Main.rand.NextFloat(0.5f, 1.5f);

                Color particleColor = Color.Lerp(coreSlagColor, outerSlagColor, Main.rand.NextFloat());

                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(spawnPosition, particleVelocity, true, // Has gravity
                Main.rand.Next(40, 70), Main.rand.NextFloat(0.8f, 1.3f), particleColor));
            }
            Projectile.Kill();
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damage)
        {
            Color coreSlagColor = new Color(247, 111, 77);
            Color outerSlagColor = new Color(242, 157, 170);

            Vector2 baseParticleDirection = new Vector2(Projectile.direction, 0).SafeNormalize(Vector2.UnitX * Projectile.direction);
            Vector2 initialSpawnPosition = target.Center + new Vector2(target.width / 2f * Projectile.direction + 15f * Projectile.direction, Main.rand.NextFloat(-target.height / 2f, target.height / 2f));


            for (int i = 0; i < 4; i++) 
            {
                Vector2 finalSpawnPosition = initialSpawnPosition + Main.rand.NextVector2Circular(1f, 1f);

                // This rotation makes it look like it's bursting out from a set point
                Vector2 particleVelocity = baseParticleDirection.RotatedByRandom(MathHelper.ToRadians(50f)) * Main.rand.NextFloat(10f, 13f);

                bool affectedByGravity = true;
                int lifetime = Main.rand.Next(20, 45); 
                float scale = Main.rand.NextFloat(.3f, .7f);

                // Simple way to get a similar effect to what I want
                // Ideally I would want there to be an inner glow + outer glow effect like with the projectile particles
                Color particleColor = Color.Lerp(coreSlagColor, outerSlagColor, Main.rand.NextFloat());

                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(finalSpawnPosition, particleVelocity, affectedByGravity, lifetime, scale, particleColor));
            }

            //WoF Spawning
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

                    if (!NPC.AnyNPCs(NPCID.WallofFlesh))
                    {
                        int playerIndex = Projectile.owner;
                        if (playerIndex >= 0 && Main.player[playerIndex].active)
                        {
                            if (Main.player[playerIndex].ZoneUnderworldHeight)
                            {
                                NPC.SpawnOnPlayer(playerIndex, NPCID.WallofFlesh);
                            }
                        }
                    }
                }
            }
        }
    }
}
