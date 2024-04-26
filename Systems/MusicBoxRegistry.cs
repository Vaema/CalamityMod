using Terraria;
using Terraria.ModLoader;

namespace CalamityMod
{
    public class MusicBoxRegistry : ModSystem
    {
        private static void AddMusicBox(string musicFile, int itemID, int tileID)
        {
            Mod calamity = CalamityMod.Instance;
            int musicID = MusicLoader.GetMusicSlot(calamity, musicFile);
            MusicLoader.AddMusicBox(calamity, musicID, itemID, tileID);
        }

        public override void PostSetupContent()
        {
            if (!Main.dedServ)
            {
                // 24APR2024: Ozzatron: unclear why these two songs are even in main mod, but I don't want to break precedent
                AddMusicBox("Sounds/Music/DraedonExoSelect", ModContent.ItemType<Items.Placeables.MusicBoxes.DraedonExoSelectMusicBox>(), ModContent.TileType<Tiles.MusicBoxes.DraedonExoSelectMusicBox>());
                AddMusicBox("Sounds/Music/DraedonTalk", ModContent.ItemType<Items.Placeables.MusicBoxes.DraedonTalkMusicBox>(), ModContent.TileType<Tiles.MusicBoxes.DraedonTalkMusicBox>());
            }
        }
    }
}
