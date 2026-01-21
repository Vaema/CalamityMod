using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss.BrainOfCthulhu;

public class TelekineticBlast : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 1;
        Projectile.damage = 0;
        Projectile.scale = 1;
        Projectile.hostile = true;
    }

    Player target => Main.player[(int)Projectile.ai[0]];
    float debuffMultiplier => Projectile.ai[1];

    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector2 dir = target.Center - Projectile.Center;
            int lifeTime = 24;
            dir /= lifeTime / 2f * 5f;
            dir *= i;
            DirectionalPulseRing pulse = new(Projectile.Center, dir, i % 2 == 0 ? Color.Red : Color.Orange, new Vector2(0.5f, 1), dir.ToRotation(), 0f, i / 5f, lifeTime + 8);
            GeneralParticleHandler.SpawnParticle(pulse);
        }

        SoundEngine.PlaySound(SoundID.Zombie105, Projectile.Center); //LC Laugh
        target.AddBuff(BuffID.Darkness, (int)Math.Round(900 * debuffMultiplier));
        target.AddBuff(BuffID.Bleeding, (int)Math.Round(900 * debuffMultiplier));
        target.AddBuff(BuffID.Confused, (int)Math.Round(60 * debuffMultiplier));
        int timeToAdd = (int)Math.Round(600 * debuffMultiplier);
        int bbIndex = target.buffType.ToList().IndexOf(ModContent.BuffType<BurningBlood>());
        if (bbIndex != -1)
            timeToAdd += target.buffTime[bbIndex];
        target.AddBuff(ModContent.BuffType<BurningBlood>(), timeToAdd);

        target.Calamity().adrenaline = 0;
    }
}
