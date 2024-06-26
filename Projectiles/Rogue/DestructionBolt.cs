using System;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class DestructionBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public ref float time => ref Projectile.ai[0];
        public float CenterX;
        public float CenterY;
        public float MouseX;
        public float MouseY;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 600;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Projectile.velocity *= 0.988f;
            if (time >= 120)
            {
                if (time == 120)
                {
                    CenterX = Projectile.Center.X;
                    CenterY = Projectile.Center.Y;
                    MouseX = Owner.Calamity().mouseWorld.X;
                    MouseY = Owner.Calamity().mouseWorld.Y;
                }
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = (new Vector2(MouseX, MouseY) - Projectile.Center).SafeNormalize(Vector2.UnitX).ToRotation() + MathHelper.PiOver2;
                Projectile.Center = new Vector2(MathHelper.Lerp(CenterX, MouseX, Utils.GetLerpValue(120, 145, time, true)), MathHelper.Lerp(CenterY, MouseY, Utils.GetLerpValue(120, 145, time, true)));
            }
            else
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            time++;
            if (time >= 145)
                Projectile.Kill();
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= 0.02f;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[1] >= 1)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DestructionStar>(), Projectile.damage, Projectile.knockBack * 5, Projectile.owner);
                if (Projectile.ai[2] == 1)
                    proj.Calamity().stealthStrike = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
