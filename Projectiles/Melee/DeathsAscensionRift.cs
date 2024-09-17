using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace CalamityMod.Projectiles.Melee
{
    public class DeathsAscensionRift : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            int ballAmt = 20;
            for (int i = 0; i < ballAmt; i++)
            {
                float offset = i * 5;
                float sizeRandomness = MathHelper.Lerp(10, 0, (float)(i / (float)ballAmt));
                float positionRandomness = 20;
                float scale = (MathHelper.Lerp(90, 10, (float)(i / (float)ballAmt)) + Main.rand.NextFloat(-sizeRandomness, sizeRandomness)) * Utils.GetLerpValue(600, 590, Projectile.timeLeft, true); ;
                StreamGougeMetaball.SpawnParticle(Projectile.Center + Vector2.UnitY * (offset + Main.rand.NextFloat(-positionRandomness, positionRandomness)) , Vector2.Zero, scale);
                StreamGougeMetaball.SpawnParticle(Projectile.Center + Vector2.UnitY * -(offset + Main.rand.NextFloat(-positionRandomness, positionRandomness)), Vector2.Zero, scale);
            }

            if (Projectile.ai[0]++ % 60 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item104 with { Pitch = 0.4f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.4f }, Projectile.Center);
                int scytheAmt = 4;
                float speed = 30;
                for (int i = 0; i < scytheAmt; i++)
                {
                    Vector2 scytheVelocity = Vector2.UnitY.RotatedBy(MathHelper.Lerp(0, 3 * MathHelper.PiOver2, (float)(i / (float)(scytheAmt - 1))) + Projectile.ai[1]) * speed;
                    int revolveTime = 70;
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, scytheVelocity, ModContent.ProjectileType<DeathsAscensionProjectile>(), (int)(Projectile.damage * 0.1f), Projectile.knockBack, Projectile.owner, ai2: revolveTime);
                    Main.projectile[p].penetrate = -1;
                }
            }

            Projectile.ai[1] += 0.1f;
        }
        public override bool? CanCutTiles() => false;
    }
}
