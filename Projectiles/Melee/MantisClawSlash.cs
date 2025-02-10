using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using CalamityMod.Items.BaseItems;
using Terraria.ModLoader;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using CalamityMod.Balancing;
using Terraria.Audio;
using CalamityMod.Items.Weapons.Melee;
using System;
using CalamityMod.Particles;
using Terraria.DataStructures;
using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityMod.Projectiles.Melee
{
    public class MantisClawSlash : ModProjectile, ILocalizedModType
    {
        public int TimerCap => 20;

        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Particles/SlashSmear";

        Color startColor;
        Color endColor;

        public override void SetDefaults()
        {
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.timeLeft = TimerCap;
            Projectile.knockBack = 2;
            Projectile.tileCollide = false;
            Projectile.width = 256;
            Projectile.height = 256;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
        }

        int dir = 1;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0f;
            Projectile.ai[2] = Main.rand.NextFloat(0.5f, 1.25f);

            if (Main.rand.NextBool(2)) dir = -1;

            // i cherry picked five specific colors to make a similar but different palette to the claws themselves
            // of those five, the projectile lerps between two, randomly chosen when the projectile is spawned
            List<Color> ColorList =
            [
                new Color(248, 197, 58),
                new Color(143, 208, 50),
                new Color(69, 114, 227),
                new Color(212, 128, 187),
                new Color(255, 140, 82),
            ];

            startColor = ColorList[Main.rand.Next(ColorList.Count)];
            endColor = ColorList[Main.rand.Next(ColorList.Count)];
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = new Rectangle((int)Projectile.Center.X - 65, (int)Projectile.Center.Y - 65, 130, 130);
        }

        public override void AI()
        {
            Projectile.velocity *= 0.9f;

            if (Projectile.timeLeft > (TimerCap / 2))
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.ai[2], 0.1f);
            }
            else
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, Projectile.ai[2], -0.1f);
            }

            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(3f) * dir);

            Projectile.ai[1]++;

            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], MathHelper.TwoPi * dir, 0.15f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
        }

        public override bool? CanDamage()
        {
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            for (float i = 0; i < 1; i += 0.33f)
            {
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, tex.Frame(), Color.Lerp(startColor, endColor, Projectile.ai[1] / TimerCap).MultiplyRGBA(new Color(255, 255, 255, 0f)),
                    Projectile.rotation - (dir == -1 ? MathHelper.ToRadians(-135f) : MathHelper.ToRadians(180f)) + Projectile.ai[0], tex.Size() / 2, MathHelper.Lerp(0.6f, 1f, i) * Projectile.scale, dir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            }

            return false;
        }
    }
}
