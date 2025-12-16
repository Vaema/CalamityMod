using CalamityMod.Buffs.DamageOverTime;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    // 13NOV2024: Ozzatron: Literally just a DirectStrike that inflicts holy flames.
    // Because it's a separate projectile, it has its own name in the localization files.
    public class WarbannerDamage : DirectStrike, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 30);
        }

    }
}
