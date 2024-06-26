using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace CalamityMod.Projectiles.Rogue
{
    public class DestructionStar : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/StarofDestruction";

        private float Radius = 47f;

        public override void SetDefaults()
        {
            Projectile.width = 94;
            Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.penetrate = 16;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            Vector2 moveToMouse = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
            if (Projectile.velocity.Length() < 6)
                Projectile.velocity += moveToMouse * 0.1f;
            else
                Projectile.velocity *= 0.8f;

            //if (Projectile.Calamity().stealthStrike)
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            /*
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.88f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
            */
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Asset<Texture2D> portal = ModContent.Request<Texture2D>("CalamityMod/Particles/Light");

            for (int i = 0; i < 4; i++)
            {
                Color auraColor = Color.Black;
                Vector2 rotationalDrawOffset = (MathHelper.TwoPi * i / 7f + Main.GlobalTimeWrappedHourly * 17f).ToRotationVector2();
                rotationalDrawOffset *= MathHelper.Lerp(3f, 5.25f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 6f) * 0.5f + 2);
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + rotationalDrawOffset, null, auraColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(portal.Value, Projectile.Center - Main.screenPosition, null, Color.Black, 0, portal.Size() * 0.5f, 1.8f * 2, SpriteEffects.None);
            Main.EntitySpriteDraw(portal.Value, Projectile.Center - Main.screenPosition, null, Color.LightGreen with { A = 0 }, 0, portal.Size() * 0.5f, 1.1f * 2, SpriteEffects.None);

            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Radius, targetHitbox);
        public override void OnKill(int timeLeft)
        {
            
        }
    }
}
