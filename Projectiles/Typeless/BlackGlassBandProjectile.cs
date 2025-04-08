using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class BlackGlassBandProjectile : DirectStrike, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 2;
            Projectile.ArmorPenetration = 25;
        }
        public override void PostAI()
        {
            if (time == 0)
            {
                SoundStyle sound = new("CalamityMod/Sounds/Custom/AuricMine", 3);
                SoundEngine.PlaySound(sound with { Volume = 0.9f, Pitch = Main.rand.NextFloat(0.15f, 0.25f), MaxInstances = -1 }, Projectile.Center);
                SoundStyle sound2 = new("CalamityMod/Sounds/Item/NullHit");
                SoundEngine.PlaySound(sound2 with { Volume = 0.6f, Pitch = Main.rand.NextFloat(0.3f, 0.45f), MaxInstances = -1 }, Projectile.Center);

                for (int i = 0; i < 18; i++)
                {
                    Particle spark = new CustomSpark(Projectile.Center, new Vector2(9, 9).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f), "CalamityMod/Particles/GlowOrbParticle", true, 35, Main.rand.NextFloat(0.6f, 0.8f), Main.rand.NextBool() ? Color.DarkSlateBlue : Color.MediumPurple, new Vector2(0.8f, 1.2f), false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                float randRot = Main.rand.NextFloat(0, MathHelper.Pi);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 position = Projectile.Center + (Vector2.UnitX * 20 * i).RotatedBy(randRot);
                    Particle spark = new CustomSpark(position, Vector2.UnitY.RotatedBy(randRot) * 0.001f, "CalamityMod/Particles/GlowSpark", false, 15, 0.03f, Color.DarkSlateBlue, new Vector2(7f, 0.2f), true, shrinkSpeed: 0.55f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 position = Projectile.Center + (Vector2.UnitY * 20 * i).RotatedBy(randRot);
                    Particle spark = new CustomSpark(position, Vector2.UnitX.RotatedBy(randRot) * 0.001f, "CalamityMod/Particles/GlowSpark", false, 15, 0.03f, Color.DarkSlateBlue, new Vector2(7f, 0.2f), true, shrinkSpeed: 0.55f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SetCrit();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player Owner = Main.player[Projectile.owner];
            if ((damageDone <= 2 || (target.life <= 0 && target.realLife == -1)) && Owner.Calamity().bGlassbandCooldown > BlackGlassBand.cooldown / 2)
                Owner.Calamity().bGlassbandCooldown = BlackGlassBand.cooldown / 2;
        }
    }
}
