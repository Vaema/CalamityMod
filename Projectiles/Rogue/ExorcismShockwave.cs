using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class ExorcismShockwave : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 14;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = RogueDamageClass.Instance;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.35f;
            int hitsToMinMult = 15;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;

            Vector2 launchVel = Utils.DirectionTo(Projectile.Center, target.Center) - Vector2.UnitY;
            float launchPower = (Projectile.Calamity().stealthStrike ? 4f : 1) * 10;
            target.MoveNPC(launchVel, launchPower, true);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float crossSize = (Projectile.Calamity().stealthStrike ? 4f : 1) * 130;
            float crosThickness = (Projectile.Calamity().stealthStrike ? 4f : 1) * 25;
            float _ = float.NaN;
            bool horizontalHit = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - Vector2.UnitX * crossSize, Projectile.Center + Vector2.UnitX * crossSize, crosThickness, ref _);
            bool verticalHit = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - Vector2.UnitY * crossSize, Projectile.Center + Vector2.UnitY * crossSize * 1.7f, crosThickness, ref _);
            return (horizontalHit || verticalHit);
        }
    }
}
