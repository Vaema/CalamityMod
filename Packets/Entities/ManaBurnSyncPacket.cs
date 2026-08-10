using System.IO;
using Terraria;

namespace CalamityMod.Packets.Entities;

internal sealed class ManaBurnSyncPacket : CalamityPacket
{
    public static ManaBurnSyncPacket Instance { get; private set; }

    public static void Send(NPC npc, int toClient = -1, int ignoreClient = -1)
    {
        if (npc is null)
            return;

        var packet = Instance.CreateBasePacket();
        packet.WriteWhoAmI(npc);
        packet.Write(npc.Calamity().manaBurn);
        packet.Write(npc.Calamity().manaBurnPeak);
        packet.Send(toClient, ignoreClient);
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        var npc = packet.ReadNPC();
        var burn = packet.ReadSingle();
        var burnPeak = packet.ReadSingle();

        if (npc is null)
            return;

        npc.Calamity().manaBurn = burn;
        npc.Calamity().manaBurnPeak = burnPeak;
    }
}
