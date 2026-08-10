using CalamityMod.Buffs.DamageOverTime;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless;

// 13NOV2024: Ozzatron: Literally just a DirectStrike that inflicts holy flames.
// Because it's a separate projectile, it has its own name in the localization files.
public class WarbannerDamage : DirectStrike, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public Player Owner => Main.player[Projectile.owner];

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<HolyFlames>(), 30);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        bool crit = Main.rand.Next(0, 100 + 1) < Owner.GetTotalCritChance(Owner.GetBestClass());
        if (crit)
            modifiers.SetCrit();
    }

}
