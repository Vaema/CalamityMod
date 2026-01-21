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

        private int BGBG0;
        private int BGBG1;
        private int BGBG2;
        private int BGBG3;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            SSBG0 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BlankPixel");
            SSBG1 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BlankPixel");
            SSBG2 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BlankPixel");
            SSBG3 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BlankPixel");

            BGBG0 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BlankPixel");
            BGBG1 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BasaltGullyBG");
            BGBG2 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BlankPixel");
            BGBG3 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/BasaltGullyBG");
        }

        public override void FillTextureArray(int[] textureSlots)
        {
            if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.RadiantReefsBiome>()) ||
                Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.PolypForestBiome>()) ||
                Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.TimelessShoresBiome>()) ||
                Main.LocalPlayer.InModBiome<BiomeManagers.GleamingBurrowsBiome>())
            {
                textureSlots[0] = SSBG0;
                textureSlots[1] = SSBG1;
                textureSlots[2] = SSBG2;
                textureSlots[3] = SSBG3;
            }
            if (!Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.RadiantReefsBiome>()) &&
                !Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.PolypForestBiome>()) &&
                !Main.LocalPlayer.InModBiome(ModContent.GetInstance<BiomeManagers.TimelessShoresBiome>()) &&
                !Main.LocalPlayer.InModBiome<BiomeManagers.GleamingBurrowsBiome>())
            {
                textureSlots[0] = BGBG0;
                textureSlots[1] = BGBG1;
                textureSlots[2] = BGBG2;
                textureSlots[3] = BGBG3;
            }
        }
    }
}
