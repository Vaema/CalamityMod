using System.IO;
using System.Reflection;
using CalamityMod.CalPlayer;
using CalamityMod.Packets;
using CalamityMod.Tiles.DraedonStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.TileEntities;

public class TELabHologramProjector : ModTileEntity
{
    public const float PopupDistance = 560f;

    public Vector2 Center => Position.ToWorldCoordinates(LabHologramProjector.Width * 8f, LabHologramProjector.Height * 8f);
    public bool PoppingUp = false;

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];

        int style = 0, alt = 0;
        TileObjectData.GetTileInfo(tile, ref style, ref alt);
        TileObjectData data = TileObjectData.GetTileData(tile.TileType, style, alt);

        int sheetSquare = 16 + data.CoordinatePadding;
        int FrameX = tile.TileFrameX / sheetSquare % data.Width;
        int FrameY = tile.TileFrameY / sheetSquare % data.Height;

        return tile.HasTile && tile.TileType == ModContent.TileType<LabHologramProjector>() && FrameX == 0 && FrameY == 0;
    }

    // Check if the hologram should become visible.
    public override void Update()
    {
        bool wasPoppingUp = PoppingUp;

        // Stop popping up by default.
        PoppingUp = false;

        // But check if a player is nearby.
        // If one is, pop up.
        Vector2 projectorCenterPos = Center;
        float distSQ = PopupDistance * PopupDistance;
        foreach (Player p in Main.ActivePlayers)
        {
            // The lack of a dead check is intentional. Dead players keep hologram boxes on for amusement.

            if (p.DistanceSQ(projectorCenterPos) < distSQ)
            {
                PoppingUp = true;
                break;
            }
        }

        if (PoppingUp != wasPoppingUp)
            SendSyncPacket();
    }

    // This code is called as a hook when the player places the Lab Hologram Projector tile so that the tile entity may be placed.
    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        // If in multiplayer, tell the server to place the tile entity and DO NOT place it yourself. That would mismatch IDs.
        // Also tell the server that you placed the 6x7 tiles that make up the Lab Hologram Projector.
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, LabHologramProjector.Width, LabHologramProjector.Height);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            return -1;
        }
        return Place(i, j);
    }

    // Sync the tile entity the moment it is place on the server.
    // This is done to cause it to register among all clients.
    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

    // If this projector breaks, anyone who's viewing it is no longer viewing it.
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
            if (mp.CurrentlyViewedHologramID == ID)
            {
                mp.CurrentlyViewedHologramID = -1;
                mp.CurrentlyViewedHologramText = string.Empty;
            }
        }
    }

    public override void NetSend(BinaryWriter writer) => writer.Write(PoppingUp);
    public override void NetReceive(BinaryReader reader) => PoppingUp = reader.ReadBoolean();

    private void SendSyncPacket()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        TELabHologramProjectorPacket.Send(this, PoppingUp);
    }
}
