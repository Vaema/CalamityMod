using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class EnchantedBladeStaffProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";

        public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.width = Projectile.height = 32;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                Dust trailDust = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.BlueFairy);
                trailDust.noGravity = true;
                trailDust.noLight = true;
                trailDust.noLightEmittence = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.timeLeft = 3;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Color.Cyan with { A = 0 } * 1.2f;
            float drawRotation = Projectile.rotation + MathHelper.PiOver2;
            Vector2 anchorPoint = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, anchorPoint, Projectile.scale, SpriteEffects.None);

            return false;
        }
    }
}
