using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.MusicBoxes
{
    public class DraedonTalkMusicBox : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
            MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Sounds/Music/DraedonTalk"), Type, ModContent.TileType<Tiles.MusicBoxes.DraedonTalkMusicBox>());
        }

        public override void SetDefaults() => Item.DefaultToMusicBox(ModContent.TileType<Tiles.MusicBoxes.DraedonTalkMusicBox>(), 0);
    }
}
