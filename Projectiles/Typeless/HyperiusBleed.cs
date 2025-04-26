using CalamityMod.Items.Ammo;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class HyperiusBleed : DirectStrike, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player Owner = Main.player[Projectile.owner];

            modifiers.HideCombatText(); // Has special combat text

            SoundEngine.PlaySound(HyperiusBullet.hit with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f), MaxInstances = 10 }, Projectile.Center);
        }
    }
}
