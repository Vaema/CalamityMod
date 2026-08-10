using System.IO;
using CalamityMod.NPCs.TownNPCs;
using Terraria;

namespace CalamityMod.Packets;

internal sealed class SyncAndroombaSolutionPacket : CalamityPacket
{
    public static SyncAndroombaSolutionPacket Instance { get; private set; }

    public static void Send(AndroombaFriendly roomba, int solType = -1, int toClient = -1, int ignoreClient = -1)
    {
        if (roomba is null)
            return;

        var packet = Instance.CreateBasePacket();
        packet.WriteWhoAmI(roomba);
        packet.Write(solType != -1 ? solType : (int)roomba.NPC.ai[3]); // Solution
        packet.Send(toClient, ignoreClient);
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        var roomba = packet.ReadModNPC<AndroombaFriendly>();
        var solution = packet.ReadInt32();

        if (roomba is null)
            return;

        if (Main.dedServ)
            AndroombaFriendly.SwapSolution(roomba.NPC.whoAmI, solution);
    }
}
