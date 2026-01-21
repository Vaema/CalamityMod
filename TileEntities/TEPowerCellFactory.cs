using System.IO;
using System.Reflection;
using CalamityMod.CalPlayer;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Packets;
using CalamityMod.Tiles.DraedonStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.TileEntities
{
    public class TEPowerCellFactory : ModTileEntity
    {
        public Vector2 Center => Position.ToWorldCoordinates(8f * PowerCellFactory.Width, 8f * PowerCellFactory.Height);
        public long Time = 0;

        internal short Stack_Internal = 0;

        public short CellStack
        {
            get => Stack_Internal;
            set
            {
                Stack_Internal = value;
                SendSyncPacket();
            }
        }

        private long CycleFrameCounter
        {
            get
            {
                long totalCycleTime = PowerCellFactory.BetweenCellDowntime + PowerCellFactory.TotalFrames * PowerCellFactory.AnimationFramerate;
                return Time % totalCycleTime;
            }
        }

        private bool IsCellFrame
        {
            get
            {
                long magicFrame = PowerCellFactory.BetweenCellDowntime + PowerCellFactory.CellCreateFrame * PowerCellFactory.AnimationFramerate + PowerCellFactory.MagicFrameDelay;
                return CycleFrameCounter == magicFrame;
            }
        }

        // Property which allows anyone to get the current animation frame of this specific factory.
        public int AnimationFrame
        {
            get
            {
                int f = (int)CycleFrameCounter;

                // The animation sticks on the last frame throughout the entire downtime period.
                if (f < PowerCellFactory.BetweenCellDowntime)
                    return PowerCellFactory.TotalFrames - 1;

                // Remove the starting downtime period for the framerate divisor calculation.
                return (f - PowerCellFactory.BetweenCellDowntime) / PowerCellFactory.AnimationFramerate;
            }
        }

        // This guarantees that this tile entity will not persist if not placed directly on the top left corner of a Power Cell Factory tile.
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<PowerCellFactory>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public override void Update()
        {
            // CIT 7OCT2024: Power Cell Factories now produce power cells faster when sleeping or using Journey's time rate multiplier.
            for (int t = 0; t < Main.desiredWorldTilesUpdateRate; t++)
            {
                ++Time;
                int maxCellStack = ModContent.GetModItem(ModContent.ItemType<DraedonPowerCell>()).Item.maxStack;
                if (IsCellFrame && CellStack < maxCellStack)
                    // The property setter will automatically send the necessary packet.
                    CellStack++;
            }
        }

        // This code is called as a hook when the player places the Power Cell Factory tile so that the tile entity may be placed.
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            // If in multiplayer, tell the server to place the tile entity and DO NOT place it yourself. That would mismatch IDs.
            // Also tell the server that you placed the 4x4 tiles that make up the Power Cell Factory.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, PowerCellFactory.Width, PowerCellFactory.Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
                return -1;
            }

            // If in single player, just place the tile entity, no problems.
            int id = Place(i, j);
            return id;
        }

        // This code is called on dedicated servers only. It is the server-side response to MessageID.TileEntityPlacement.
        // When the server receives such a message from a client, it sends a MessageID.TileEntitySharing to all clients.
        // This will cause them to Place the tile entity locally at that position, all with exactly the same ID.
        public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

        // If this factory breaks, anyone who's viewing it is no longer viewing it.
        public override void OnKill()
        {
            foreach (Player p in Main.ActivePlayers)
            {
                // Use reflection to stop TML from spitting an error here.
                // Try-catching will not stop this error, TML will print it to console anyway. The error is harmless.
                ModPlayer[] mpStorageArray = (ModPlayer[])typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(p);
                if (mpStorageArray.Length == 0)
                    continue;

                CalamityPlayer mp = p.Calamity();
                if (mp.CurrentlyViewedFactoryID == ID)
                    mp.CurrentlyViewedFactoryID = -1;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["time"] = Time;
            tag["cells"] = Stack_Internal;
        }

        public override void LoadData(TagCompound tag)
        {
            Time = tag.GetLong("time");
            Stack_Internal = tag.GetShort("cells");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(Stack_Internal);
        }

        public override void NetReceive(BinaryReader reader)
        {
            Time = reader.ReadInt64();
            Stack_Internal = reader.ReadInt16();
        }

        private void SendSyncPacket()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            TEPowerCellFactoryPacket.Send(this, Time, Stack_Internal);
        }
    }
}
