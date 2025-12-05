using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture.Monoliths
{
    public class BossRushMonolithTile : BaseMonolith
    {
        public override int TileWidth => 4;
        public override int TileHeight => 4;
        public override int AnimationFrameCount => 5;
        public override int AnimationDelay => 8;
        public override int CursorItemType => ModContent.ItemType<BossRushMonolith>();

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                GlowMask = ModContent.Request<Texture2D>($"{Texture}_Glow");
            }

            RegisterItemDrop(ModContent.ItemType<BossRushMonolith>());
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 18 };
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 4, 0);
            
            AnimationFrameHeight = TileObjectData.newTile.CoordinateFullHeight;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(64, 8, 12));

            DustType = (int)CalamityDusts.Brimstone;
        }

        public override void NearbyEffects(int i, int j, bool closer, bool monolithEnabled, Player localPlayer)
        {
            if (!monolithEnabled)
                return;

            if (localPlayer is not null && localPlayer.active)
                localPlayer.Calamity().monolithBossRushShader = 30;
        }
    }
}
