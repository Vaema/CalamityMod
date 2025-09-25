using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace CalamityMod.Projectiles.Enemy
{
    public class ClamBubbleBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 20;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            // Bubbles
            if (Main.rand.NextBool())
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 9 + Main.rand.Next(7);
                bubble.scale = Main.rand.NextFloat(0.6f, 1f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Bubbles
            for (int i = 0; i < 10; i++)
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(60f)) * 0.3f, 411);
                bubble.timeLeft = 9 + Main.rand.Next(7);
                bubble.scale = Main.rand.NextFloat(0.6f, 1f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }
        }
    }
}
