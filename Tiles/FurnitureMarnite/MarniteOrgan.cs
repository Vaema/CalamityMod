using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;


namespace CalamityMod.Tiles.FurnitureMarnite
{
    public class MarniteOrgan : ModTile
    {
        public static readonly SoundStyle MarniteOrganSound = new("CalamityMod/Sounds/Music/MarniteOrgan", 1);
        public override void SetStaticDefaults() => this.SetUpPiano(ModContent.ItemType<Items.Placeables.FurnitureMarnite.MarniteOrgan>(), true);

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Granite, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<Items.Placeables.FurnitureMarnite.MarniteOrgan>();
        }
        public override bool RightClick(int i, int j)
        {
            SoundEngine.PlaySound(MarniteOrganSound);
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
