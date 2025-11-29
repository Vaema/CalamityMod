using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class ModLavaStyleSystem : ModSystem
    {
        public static ModLavaStyle CurrentLavaStyle => LavaStyles[LavaStyle];

        public static void ModifyLightSetup(int i, int j, int style, ref float r, ref float g, ref float b)
        {
            if (LavaStyles[style] is ModLavaStyle styles)
            {
                styles.ModifyLight(i, j, ref r, ref g, ref b);
            }
        }

        public static void DrawColorSetup(int x, int y, int type, ref VertexColors liquidColor, bool isSlope = false)
        {
            if (LavaStyles[type] is ModLavaStyle styles)
            {
                styles.DrawColor(x, y, ref liquidColor, isSlope);
            }
        }

        public static int GetDropletGoreID(int oldID = -1)
        {
            if (CurrentLavaStyle is ModLavaStyle lavaStyle)
            {
                return lavaStyle.GetDropletGore();
            }

            return (oldID >= 0) ? oldID : GoreID.LavaDrip;
        }

        public static int GetSplashDustID(int oldID = -1)
        {
            if (CurrentLavaStyle is ModLavaStyle lavaStyle)
            {
                return lavaStyle.GetSplashDust();
            }

            return (oldID >= 0) ? oldID : DustID.Lava;
        }

        public static void InflictDebuff(Player player, int onFireTime)
        {
            if (CurrentLavaStyle is ModLavaStyle lavaStyle)
            {
                lavaStyle.InflictDebuff(player, onFireTime);
            }
        }
    }
}
