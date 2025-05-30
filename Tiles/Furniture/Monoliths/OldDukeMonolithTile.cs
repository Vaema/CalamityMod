using CalamityMod.Buffs.Potions;
using CalamityMod.Dusts;
using CalamityMod.ForegroundDrawing.LoopingTextures;
using CalamityMod.Items.Dyes;
using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture.Monoliths
{
    public class OldDukeMonolithTile : BaseMonolith
    {
        public override int TileWidth => 4;
        public override int TileHeight => 8;
        public override int AnimationFrameCount => 24;
        public override int AnimationDelay => 6;
        public override int CursorItemType => ModContent.ItemType<OldDukeMonolith>();

        public static Asset<Texture2D> Numbers;
        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                GlowMask = ModContent.Request<Texture2D>($"{Texture}_Glow");
                //Numbers = ModContent.Request<Texture2D>("CalamityMod/Tiles/Furniture/Monoliths/ExoObeliskText", AssetRequestMode.AsyncLoad);
            }

            RegisterItemDrop(CursorItemType);
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 8;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.Origin = new Point16(1, 4);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
            
            AnimationFrameHeight = 8 * 18;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16, 16, 16];
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(54, 54, 54));

            DustType = DustID.TerraBlade;
        }

        public override void NearbyEffects(int i, int j, bool closer, bool monolithEnabled, Player localPlayer)
        {
            if (!monolithEnabled)
                return;

            if (localPlayer is not null && localPlayer.active)
            {
                localPlayer.GetModPlayer<NuclearTorrentPlayer>().ShouldDisplayTorrentMonolith = true;
            }
        }
    }
}
