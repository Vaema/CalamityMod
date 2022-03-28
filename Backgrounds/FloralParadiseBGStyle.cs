using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Backgrounds
{
    public class FloralParadiseBGStyle : ModUgBgStyle
    {
        public override bool ChooseBgStyle() => Main.LocalPlayer.InFloralParadise();

        public override void FillTextureArray(int[] textureSlots)
        {
            // Corresponds to underground jungle backgrounds. Something custom could probably be made, but for what it is it works sufficiently for the minibiome.
            for (int i = 0; i <= 3; i++)
                textureSlots[i] = i + 153;
        }
    }
}
