using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.Plates
{
    public class Onyxplate : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Plates/OnyxplateGlow";

        internal static GrayscaleTexture1D PulseGradient;
        public override void SetupStatic()
        {
            PulseGradient = new("CalamityMod/Tiles/Plates/OnyxplatePulse");

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = CommonCalamitySounds.PlatingMine;
            MineResist = 1f;
            DustType = DustID.ShadowbeamStaff;
            AddMapEntry(new Color(182, 28, 232));
        }

        public override void OnUnload()
        {
            PulseGradient?.Unload();
            PulseGradient = null;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            float brightness = PulseGradient.GetRepeat((int)Main.GameUpdateCount);
            brightness = 0.04f + (brightness * 0.31f);
            return Color.White * brightness;
        }
    }
}
