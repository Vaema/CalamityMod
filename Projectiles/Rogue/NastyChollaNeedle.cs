using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NastyChollaNeedle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -2;
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
            //Rotating 45 degrees if shooting right
            if (Projectile.spriteDirection == 1)
            {
                Projectile.rotation += MathHelper.ToRadians(45f);
            }
            //Rotating 45 degrees if shooting right
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation -= MathHelper.ToRadians(45f);
            }
            Projectile.velocity.X *= 0.9995f;
            Projectile.velocity.Y = Projectile.velocity.Y + 0.01f;
        }

        //So you can stick a needle up the Tinkerer's ass
        public override bool? CanHitNPC(NPC target) => target.type != NPCID.DD2EterniaCrystal && !target.immortal && !target.dontTakeDamage;

        // 18NOV2024: Ozzatron: Nasty Cholla needles are not allowed to deal more than 1 damage under any circumstances.
        // This prevents them from becoming demonchungus when given flat damage boosts like Copper armor or Thorium's The Ring.
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetMaxDamage(1);
    }
}
