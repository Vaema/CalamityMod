using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.MusicBoxes;

[LegacyName("DraedonsAmbienceMusicBox")]
public class DraedonExoSelectMusicBox : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.CanGetPrefixes[Type] = false;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
        MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Sounds/Music/DraedonExoSelect"), Type, ModContent.TileType<Tiles.MusicBoxes.DraedonExoSelectMusicBox>());
    }

    public override void SetDefaults() => Item.DefaultToMusicBox(ModContent.TileType<Tiles.MusicBoxes.DraedonExoSelectMusicBox>(), 0);
}
