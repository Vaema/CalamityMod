using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Packets;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class Slagfire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override bool CanHitPlayer(Player target) => false;
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

            Color color = new(254, 63, 63);

            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkPosition = Projectile.Center - (Projectile.velocity / 3f * i);

                Vector2 sparkVelocity = -Projectile.velocity * 0.01f * Main.rand.NextFloat(0.5f, 1.5f);

                Particle spark = new CustomSpark(sparkPosition, sparkVelocity, "CalamityMod/Particles/BloomLineFade", false, 6, 0.04f, color * 0.85f, new Vector2(0.45f, 0.9f), shrinkSpeed: 0.4f);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 sparkPosition = Projectile.Center + Main.rand.NextVector2Circular(15, 15);
                Vector2 sparkVelocity = -Projectile.velocity * Main.rand.NextFloat(0.01f, 0.045f); // Backward velocity

                Particle subtleSpark = new CustomSpark(sparkPosition, sparkVelocity, "CalamityMod/Particles/BloomLineFade", false, 8, 0.02f, color * 0.5f, new Vector2(0.5f, 0.8f), shrinkSpeed: 0.3f);
                GeneralParticleHandler.SpawnParticle(subtleSpark);
            }

            // Falling particles to appear as lava droplets, similar to speed blaster's falling dust
            if (Main.rand.NextBool(50))
            {
                Vector2 spawnHere = Projectile.position;
                spawnHere.Y = Projectile.Bottom.Y + 3f; // A little down

                Vector2 particleVelocity = new Vector2(Main.rand.Next(-1, 1), 3);

                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(spawnHere, particleVelocity, true, // Affected by gravity
                Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.65f), new(220, 138, 138)));
            }

            if (Projectile.localAI[1] >= 10f)
                Projectile.velocity.Y += 0.2f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.type == NPCID.Guide)
                modifiers.FinalDamage *= 10f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Color innerColor = new(230, 198, 198);
            Color outterColor = new(248, 63, 63);

            Vector2 baseParticleDirection = new Vector2(Projectile.direction, 0).SafeNormalize(Vector2.UnitX * Projectile.direction);

            for (int i = 0; i < 4; i++)
            {
                // This rotation makes it look like it's bursting out from a set point
                Vector2 particleVelocity = baseParticleDirection.RotatedByRandom(MathHelper.ToRadians(50f)) * Main.rand.NextFloat(10f, 13f);

                bool affectedByGravity = true;
                int lifetime = Main.rand.Next(20, 45);
                float scale = Main.rand.NextFloat(.3f, .7f);

                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(Projectile.Center, particleVelocity, affectedByGravity, lifetime, scale * 1.15f, outterColor));
                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(Projectile.Center, particleVelocity, affectedByGravity, lifetime, scale * 0.75f, innerColor));
            }
            Projectile.Kill();
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damage)
        {
            Color innerColor = new(230, 198, 198);
            Color outterColor = new(248, 63, 63);

            Vector2 baseParticleDirection = new Vector2(Projectile.direction, 0).SafeNormalize(Vector2.UnitX * Projectile.direction);
            Vector2 initialSpawnPosition = target.Center + new Vector2(target.width / 2f * Projectile.direction + 4f * Projectile.direction, Main.rand.NextFloat(-target.height / 2f, target.height / 2f));

            for (int i = 0; i < 4; i++) 
            {
                Vector2 finalSpawnPosition = initialSpawnPosition + Main.rand.NextVector2Circular(1f, 1f);

                // This rotation makes it look like it's bursting out from a set point
                Vector2 particleVelocity = baseParticleDirection.RotatedByRandom(MathHelper.ToRadians(50f)) * Main.rand.NextFloat(10f, 13f);

                bool affectedByGravity = true;
                int lifetime = Main.rand.Next(20, 45); 
                float scale = Main.rand.NextFloat(.3f, .7f);

                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(finalSpawnPosition, particleVelocity, affectedByGravity, lifetime, scale * 1.15f, outterColor));
                GeneralParticleHandler.SpawnParticle(new WaterFlavoredParticle(finalSpawnPosition, particleVelocity, affectedByGravity, lifetime, scale * 0.75f, innerColor));
            }

            //WoF Spawning
            if (target.type == NPCID.Guide)
            {
                if (target.life <= 0)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            SpawnBossOnPositionPacket.Send((int)Main.player[Projectile.owner].Center.X, (int)Main.player[Projectile.owner].Center.Y, NPCID.WallofFlesh, Main.player[Projectile.owner]);

                        else
                            NPC.SpawnWOF(Main.player[Projectile.owner].Center);
                    }
                }
            }
        }
    }
}
