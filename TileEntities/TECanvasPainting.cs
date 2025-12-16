using CalamityMod.Packets;
using CalamityMod.Tiles.Furniture.Paintings;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.TileEntities
{
    public class TECanvasPainting : ModTileEntity
    {
        public Vector2 framePosition = new Vector2(0, 0);
        public float scale = 1f;

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && TileLoader.GetTile(tile.TileType) is BaseCanvasPainting && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            // WHY do I need to subtract 1 from both????
            // Todo: Figure out what the heckity heckering heckling heckereckering heckly heccing hecky heck heck is happening here
            int iMinus = i - 1;
            int jMinus = j - 1;

            // If in multiplayer, tell the server to place the tile entity and DO NOT place it yourself. That would mismatch IDs.
            // Also tell the server that you placed the 5x5 tiles that make up the painting.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, iMinus, jMinus, 5, 5);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, iMinus, jMinus, Type);
                return -1;
            }

            // If in single player, just place the tile entity
            int id = Place(iMinus, jMinus);
            return id;
        }

        public override void OnNetPlace()
        {
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(framePosition.X);
            writer.Write(framePosition.Y);
            writer.Write(scale);
        }

        public override void NetReceive(BinaryReader reader)
        {
            framePosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            scale = reader.ReadSingle();
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("posX", framePosition.X);
            tag.Add("posY", framePosition.Y);
            tag.Add("scale", scale);
        }

        public override void LoadData(TagCompound tag)
        {
            framePosition = new Vector2(tag.Get<float>("posX"), tag.Get<float>("posY"));
            scale = tag.Get<float>("scale");
        }

        public void SendSyncPacket()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            TECanvasPaintingPacket.Send(this, framePosition.X, framePosition.Y, scale);
        }
    }
}
