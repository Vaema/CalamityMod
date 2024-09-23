using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Backgrounds
{
    public class SunkenSeaBGStyle : ModUndergroundBackgroundStyle
    {
        private int SSBG0;
        private int SSBG1;
        private int SSBG2;
        private int SSBG3;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            SSBG0 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/SunkenSeaBG0");
            SSBG1 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/SunkenSeaBG1");
            SSBG2 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/SunkenSeaBG2");
            SSBG3 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/SunkenSeaBG3");
        }

        public override void FillTextureArray(int[] textureSlots)
        {
            textureSlots[0] = SSBG0;
            textureSlots[1] = SSBG1;
            textureSlots[2] = SSBG2;
            textureSlots[3] = SSBG3;
        }
    }
}
