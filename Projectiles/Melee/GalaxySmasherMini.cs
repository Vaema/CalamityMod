using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee;

public class GalaxySmasherMini : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Melee";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
    public float radius = 50f;
    public int time = 0;
    public bool homing = false;
    public NPC targeted;

    public override void SetDefaults()
    {
        Projectile.width = 90;
        Projectile.height = 90;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 500;
        Projectile.extraUpdates = 3;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI()
    {
        List<Color> eColors =
        [
            Color.Aqua,
            Color.Magenta,
        ];
        float rate = (Main.GlobalTimeWrappedHourly * 20);
        int colorIndex = (int)(rate / 2 % eColors.Count);
        Color currentColor = eColors[colorIndex];
        Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
        Color usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

        if (time % 2 == 0)
        {
            Particle spark = new CustomSpark(Projectile.Center, -Projectile.velocity * 0.05f, "CalamityMod/Particles/SmallBloom", false, 6, 0.2f, usedColor, new Vector2(Utils.Remap(Projectile.velocity.Length(), 0, 5, 1, 0.6f, true), 1f), true, false, 0, false, false, Utils.Remap(Projectile.velocity.Length(), 0, 5, 0, 0.9f, true));
            GeneralParticleHandler.SpawnParticle(spark);
        }
        GalaxyMetaball.SpawnParticle(Projectile.Center, -Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.2f, 0.7f), 30f * Main.rand.NextFloat(0.9f, 1f));
        
        if (time > 45 || homing)
        {
            homing = true;
            if (Projectile.ai[2] != -5)
                targeted = Main.npc[(int)Projectile.ai[2]];
            if (targeted == null || !targeted.CanBeChasedBy(Projectile, false) || !targeted.active)
            {
                targeted = Projectile.Center.ClosestNPCAt(1000);
                Projectile.ai[2] = -5;
            }

            if (targeted != null)
            {
                CalamityUtils.HomeInOnSelectedNPC(Projectile, targeted, true, 0.18f, 25, 0.99f, accelerate: true);
                Projectile.extraUpdates = 3;
            }
        }
        else
        {
            Projectile.velocity += Projectile.velocity.RotatedBy(0.24f * Projectile.ai[1]) * 0.04f;
            Projectile.extraUpdates = 2;
        }

        time += (homing ? -1 : 1);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 60);
        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Projectile.velocity * 3) * Main.rand.NextFloat(0.3f, 1f), 0, default, Main.rand.NextFloat(0.65f, 1f));
            dust.noGravity = true;
            dust.color = Main.rand.NextBool() ? Color.Magenta : Color.Aqua;
        }

        if (target == targeted)
            Projectile.Kill();
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => homing ? CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox) : false;
}
