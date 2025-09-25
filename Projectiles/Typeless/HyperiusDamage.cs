using CalamityMod.Items.Ammo;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class HyperiusDamage : DirectStrike, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player Owner = Main.player[Projectile.owner];

            modifiers.HideCombatText(); // Has special combat text
            modifiers.SourceDamage *= 0;
            modifiers.FinalDamage.Flat = Projectile.damage; // Always does the exact damage of the hit so that it doesn't double scale with defense and dr

            SoundEngine.PlaySound(HyperiusBullet.hit with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f), MaxInstances = 10 }, Projectile.Center);
        }
    }
}
