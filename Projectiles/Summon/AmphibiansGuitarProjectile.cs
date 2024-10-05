using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class AmphibiansGuitarProjectile : ModProjectile, ILocalizedModType
    {
        public override string LocalizationCategory => "Projectiles.Summon";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public bool IsHatNote => Projectile.ai[0] != 0f;

        public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.timeLeft = 300;
            Projectile.localNPCHitCooldown = 10;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= IsHatNote ? 1.5f : 1f;
    }
}
