using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
namespace CalamityMod.Projectiles.Magic
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
            for (int i = 0; i < 3; i++)
            {
                Vector2 positionDelta = Projectile.velocity / 3f * i;
                int spawnDelta = 14;
                int dustIdx = Dust.NewDust(new Vector2(Projectile.position.X + spawnDelta, Projectile.position.Y + spawnDelta), Projectile.width - spawnDelta * 2, Projectile.height - spawnDelta * 2, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 1f);
                Dust dust = Main.dust[dustIdx];
                dust.noGravity = true;
                dust.velocity *= 0.1f;
                dust.velocity += Projectile.velocity * 0.5f;
                dust.position -= positionDelta;
            }
            if (Main.rand.NextBool(8))
            {
                int spawnDelta = 16;
                int dustIdx = Dust.NewDust(new Vector2(Projectile.position.X + spawnDelta, Projectile.position.Y + spawnDelta), Projectile.width - spawnDelta * 2, Projectile.height - spawnDelta * 2, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 0.5f);
                Main.dust[dustIdx].velocity *= 0.25f;
                Main.dust[dustIdx].velocity += Projectile.velocity * 0.5f;
            }

            if (Projectile.localAI[1] >= 10f)
                Projectile.velocity.Y += 0.075f;
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
