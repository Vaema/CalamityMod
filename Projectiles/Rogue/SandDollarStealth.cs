using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class SandDollarStealth : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Typeless/SandCloakVeil";

        private const float Radius = 96f;
        private const int StartTime = 180;
        private bool shouldDie = false;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 28;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = StartTime;
            Projectile.Opacity = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile disk = Main.projectile[(int)Projectile.ai[0]];
            Projectile.rotation += 0.025f;

            if (disk.active)
            {
                if (!shouldDie)
                {
                    Projectile.Center = disk.Center + disk.velocity;
                    Projectile.timeLeft = 20;
                }

            }
            else
                shouldDie = true;

            if (Projectile.timeLeft >= 20)
                Projectile.Opacity += 0.05f;
            else
                Projectile.Opacity -= 0.05f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (target.Center.X > Projectile.Center.X).ToDirectionInt();
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, Radius, targetHitbox);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float scaleStep = 0.05f;
            Color drawCol = Projectile.GetAlpha(Color.Lerp(lightColor, Color.White, 0.5f));
            float drawTransparency = 0.2f;

            if (Projectile.timeLeft > StartTime - 20)
                drawTransparency *= Utils.GetLerpValue(StartTime - 20, StartTime, Projectile.timeLeft, true);
            else if (Projectile.timeLeft < 20)
                drawTransparency *= Utils.GetLerpValue(0, 20, Projectile.timeLeft, true);

            for (int i = 0; i < 20; i++)
            {
                float rotation = (Projectile.rotation + scaleStep * i) * (i % 2 == 0).ToDirectionInt();
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, drawCol * drawTransparency, rotation, tex.Size() / 2f, (Radius * 0.0044f) - (i * scaleStep), SpriteEffects.None);
            }
            return false;
        }
    }
}
