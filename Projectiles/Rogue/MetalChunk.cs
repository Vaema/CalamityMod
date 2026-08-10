using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class MetalChunk : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Items/Weapons/Rogue/MetalMonstrosity";

    public bool StuckInEnemy = false;
    public int StealthShardTimer = 0;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.ignoreWater = true; //Its hella heavy so ofc
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 60;
    }

    public override void AI()
    {
        if (!StuckInEnemy) // Gravity and rotation rotate if not stuck
        {
            Projectile.velocity.Y += 0.11f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            Projectile.rotation += 0.14f * Projectile.direction;
        }
            
        if (Projectile.Calamity().stealthStrike)
            Projectile.StickyProjAI(10);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.Calamity().stealthStrike)
            StuckInEnemy = true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Projectile.Calamity().stealthStrike)
            Projectile.ModifyHitNPCSticky(1);
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);

        // Spiky balls and shards on death
        for (int i = 0; i < 3; i++)
        {
            Vector2 sVelocity = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * 4.5f;
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, sVelocity, ProjectileID.SpikyBall, (int)(Projectile.damage * 0.3), 0f, Projectile.owner, 0f, 0f);
            proj.DamageType = RogueDamageClass.Instance;
            proj.timeLeft = 600;
            proj.usesLocalNPCImmunity = true;
            proj.localNPCHitCooldown = 20;
            sVelocity = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * 3f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, sVelocity, ModContent.ProjectileType<MetalShard>(), (int)(Projectile.damage * 0.3), 0f, Projectile.owner, 0f, 0f);
        }

        // Bunch of shards on stealth strike death
        if (Projectile.Calamity().stealthStrike)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 shardVel = (Main.npc[(int)Projectile.ai[1]].Center - Projectile.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.Pi / 6f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shardVel, ModContent.ProjectileType<MetalShard>(), (int)(Projectile.damage * 0.15f), 0f, Projectile.owner);
            }
        }

        // Dust
        for (int i = 0; i < 15; i++)
        {
            Dust.NewDust(Projectile.Center, 1, 1, DustID.Lead, 0f, 0f, 0, default, 1.1f);
        }
    }
}
