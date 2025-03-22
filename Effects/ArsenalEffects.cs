using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using CalamityMod.TileEntities;
using CalamityMod.Tiles.DraedonSummoner;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Effects
{
    public class ArsenalEffects
    {
        public static int ArsenalDust = 278; // Colorable firework dust, simple and effective. Most visuals use a mix of the energy respective dust and this

        // Plasma
        public static int ArsenalPlamaDust = ModContent.DustType<SquashDustTileTouch>();
        public static Color ArsenalPlamaColor = Color.Lerp(Color.Green, Color.Chartreuse, 0.35f);
        // Laser
        public static int ArsenalLaserDust = ModContent.DustType<SquashDust>();
        public static Color ArsenalLaserColor = Color.Lerp(Color.Crimson, Color.Firebrick, 0.35f);
        // Pulse
        public static int ArsenalPulseDust = ModContent.DustType<SquashDustHollow>();
        public static Color ArsenalPulseColor = Color.Lerp(Color.DarkOrchid, Color.Magenta, 0.35f);
        // Electric
        public static int ArsenalElectricDust = ModContent.DustType<LightDust>();
        public static Color ArsenalElectricColor = Color.Lerp(Color.Aqua, Color.Aquamarine, 0.35f);
        // Gauss
        public static int ArsenalGaussDust = ModContent.DustType<SquareDust>();
        public static Color ArsenalGaussColor = Color.Lerp(Color.Yellow, Color.Goldenrod, 0.35f);
    }
}
