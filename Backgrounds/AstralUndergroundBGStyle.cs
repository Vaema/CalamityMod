using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Backgrounds
{
    public class AstralUndergroundBGStyle : ModUndergroundBackgroundStyle
    {
        private int AUBG0;
        private int AUBG1;
        private int AUBG2;
        private int AUBG3;

        private int AUBG4_0;
        private int AUBG4_1;
        private int AUBG4_2;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            AUBG0 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG0");
            AUBG1 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG1");
            AUBG2 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG2");
            AUBG3 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG3");

            AUBG4_0 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG4_0");
            AUBG4_1 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG4_1");
            AUBG4_2 = BackgroundTextureLoader.GetBackgroundSlot("CalamityMod/Backgrounds/AstralUG4_2");
        }

        public override void FillTextureArray(int[] textureSlots)
        {
            textureSlots[0] = AUBG0;
            textureSlots[1] = AUBG1;
            textureSlots[2] = AUBG2;
            textureSlots[3] = AUBG3;
            textureSlots[4] = Main.hellBackStyle switch
            {
                0 => AUBG4_0,
                1 => AUBG4_1,
                _ => AUBG4_2
            };
        }
    }
}
