using CalamityMod.Items.Placeables.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    public class TheMonumentTileEntity : ModTileEntity
    {
        public override bool IsTileValidForEntity(int x, int y)
        {
            var tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<TheMonumentTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public static bool IsInArea(Rectangle tileArea)
        {
            foreach (var (pos, tileEntity) in ByPosition)
            {
                if (tileEntity is not TheMonumentTileEntity)
                {
                    continue;
                }

                var monumentArea = new Rectangle(pos.X, pos.Y, TheMonumentTile.TileWidth, TheMonumentTile.TileHeight);
                if (monumentArea.Intersects(tileArea))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class TheMonumentTile : ModTile
    {
        internal static int TileWidth => 7;
        internal static int TileHeight => 8;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = TileWidth;
            TileObjectData.newTile.Height = TileHeight;
            TileObjectData.newTile.Origin = new Point16(3, 7);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<TheMonumentTileEntity>().Generic_HookPostPlaceMyPlayer;

            TileObjectData.addTile(Type);
            AddMapEntry(new Color(239, 205, 54), CalamityUtils.GetItemName<TheMonument>());
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Gold);
            return false;
        }
    }

    internal sealed class TheMonumentTileEntityPorter : ModSystem
    {
        private bool hasPortedTheMonument;

        public override void SaveWorldData(TagCompound tag)
        {
            base.SaveWorldData(tag);

            tag[nameof(hasPortedTheMonument)] = hasPortedTheMonument;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            base.LoadWorldData(tag);

            hasPortedTheMonument = tag.TryGet<bool>(nameof(hasPortedTheMonument), out var value) ? value : false;
        }

        // tModLoader only calls this on servers and single player clients,
        // so we don't need to do it ourselves.
        public override void PostWorldLoad()
        {
            base.PostWorldLoad();

            if (hasPortedTheMonument)
            {
                return;
            }

            var cachedType = ModContent.TileType<TheMonumentTile>();
            for (var x = 0; x < Main.maxTilesX; x += TheMonumentTile.TileWidth)
            {
                for (var y = 0; y < Main.maxTilesY; y += TheMonumentTile.TileHeight)
                {
                    var tile = Framing.GetTileSafely(x, y);
                    if (!tile.HasTile || tile.TileType != cachedType)
                    {
                        continue;
                    }

                    AddTheMonumentTileEntityIfItsThereYo(tile, x, y);
                }
            }

            hasPortedTheMonument = true;
        }

        private void AddTheMonumentTileEntityIfItsThereYo(Tile tile, int x, int y)
        {
            x -= tile.TileFrameX / 16 % TheMonumentTile.TileWidth;
            y -= tile.TileFrameY / 16 % TheMonumentTile.TileHeight;

            // Shouldn't ever happen, but better safe than sorry.
            if (!ModContent.GetInstance<TheMonumentTileEntity>().IsTileValidForEntity(x, y))
            {
                Mod.Logger.Warn($"Failed to place TheMonument tile entity near the coordinates: {x}, {y}");
                return;
            }
        
            ModContent.GetInstance<TheMonumentTileEntity>().Place(x, y);
        }
    }
}
