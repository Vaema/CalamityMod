using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless;

public class DryadsTearBurst : BasicBurst, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.DamageType = DamageClass.Ranged;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        pushVelocity = Utils.DirectionTo(Projectile.Center, target.Center);
        float minMult = Projectile.ai[1];
        int hitsToMinMult = (int)Projectile.ai[2];
        float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
        modifiers.SourceDamage *= damageMult;
        
        if (Projectile.localAI[0] == 0)
            modifiers.DisableCrit();
        else
            modifiers.SetCrit();

        if (customKnockback != 0)
            target.MoveNPC(pushVelocity, customKnockback, hasStongDisplacement, Main.player[Projectile.owner]);
    }
}
