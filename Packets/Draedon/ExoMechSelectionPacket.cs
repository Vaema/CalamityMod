using System.IO;
using CalamityMod.World;

namespace CalamityMod.Packets
{
    internal sealed class ExoMechSelectionPacket : CalamityPacket
    {
        public static ExoMechSelectionPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.ExoMechSelection;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write((int)CalamityWorld.DraedonMechToSummon);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            CalamityWorld.DraedonMechToSummon = (ExoMech)packet.ReadInt32();
        }
    }
}
