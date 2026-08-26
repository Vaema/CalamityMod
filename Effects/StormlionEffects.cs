using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Effects;

public class StormlionEffects
{
    // Energy
    public static int EnergyDust = ModContent.DustType<LightDust>();
    public static Color EnergyColor = new(5, 187, 177);
    // Flesh
    public static int FleshDust = 192;
    public static Color FleshColor = new(171, 113, 91);

    // Sounds
    public static readonly SoundStyle Hit = new("CalamityMod/Sounds/NPCHit/StormlionAltHit");
    public static readonly SoundStyle Killed = new("CalamityMod/Sounds/NPCKilled/StormlionAltDeath");
    public static readonly SoundStyle Attack = new("CalamityMod/Sounds/Custom/StormlionAltShoot");
    public static readonly SoundStyle Idle1 = new("CalamityMod/Sounds/Custom/StormlionAltIdle1");
    public static readonly SoundStyle Idle2 = new("CalamityMod/Sounds/Custom/StormlionAltIdle2");
}
