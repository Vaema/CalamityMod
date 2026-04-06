using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture.Monoliths
{
    public class PurpleDistortedMonolithTile : BaseMonolith
    {
        public override int TileWidth => 5;
        public override int TileHeight => 5;
        public override int AnimationFrameCount => 6;
        public override int AnimationDelay => 8;
        public override int CursorItemType => ModContent.ItemType<PurpleDistortedMonolith>();
        public override void SetStaticDefaults()
        {
            RegisterItemDrop(ModContent.ItemType<PurpleDistortedMonolith>());
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.Origin = new Point16(2, 4);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 18 };
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 5, 0);

            AnimationFrameHeight = TileObjectData.newTile.CoordinateFullHeight;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(50, 127, 209));

            DustType = (int)CalamityDusts.PurpleCosmilite;
        }

        public override void NearbyEffects(int i, int j, bool closer, bool monolithEnabled, Player localPlayer)
        {
            if (!monolithEnabled)
                return;

            if (localPlayer is not null && localPlayer.active)
                localPlayer.Calamity().monolithDevourerPShader = 30;
        }
    }
}
