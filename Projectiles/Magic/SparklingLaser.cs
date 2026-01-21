using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    [PierceResistException]
    public class SparklingLaser : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public bool isSplit => Projectile.ai[1] == 5;
        public ref NPC targeted => ref Main.npc[(int)Projectile.ai[2]];
        public Color mainColor = Color.Cyan;
        public override void SetDefaults()
        {
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.ArmorPenetration = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 3;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (time == 0)
            {
                mainColor = Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue;
            }
            if (targetDist < 1400)
            {
                float beamSize = (isSplit ? 0.4f : 1f);
                Particle beamBody = new CustomSpark(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 65, Projectile.velocity * 0.05f, "CalamityMod/Particles/Crack", false, (int)(7 * (isSplit ? 2 : 1)), 0.15f * beamSize, mainColor * 0.7f, new Vector2(0.7f, 1.4f), true, true, extraRotation: MathHelper.ToRadians(180), shrinkSpeed: (isSplit ? 0.4f : 1.1f), glowCenterScale: 0.7f, glowOpacity: 0.4f);
                GeneralParticleHandler.SpawnParticle(beamBody);
            }
            if (isSplit)
            {
                Projectile.velocity *= 0.98f;
            }
            else if (Projectile.velocity.Length() < 9)
            {
                Projectile.velocity *= 1.01f;
                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.035f);
            }
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!isSplit)
            {
                Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 7;
                Projectile splitProj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (shootDirection).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.8f, 1.2f), ModContent.ProjectileType<SparklingLaser>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0, 5, target.whoAmI);
                splitProj.timeLeft = 120;
                splitProj.penetrate = -1;
                splitProj.extraUpdates = 2;
                for (int k = 0; k < 3; k++)
                {
                    Vector2 shootVel = (shootDirection).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 1.8f);

                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), shootVel);
                    dust2.scale = Main.rand.NextFloat(0.9f, 1.15f);
                    dust2.noGravity = true;
                    dust2.color = Main.rand.NextBool() ? Color.DodgerBlue : Color.Cyan;
                }
            }
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), (isSplit ? 60 : 120));

            float minMult = 0.25f;
            int hitsToMinMult = 7;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override bool? CanHitNPC(NPC target) => (target == targeted && isSplit) ? false : null;
    }
}
