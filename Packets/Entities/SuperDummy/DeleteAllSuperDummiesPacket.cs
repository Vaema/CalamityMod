using System.IO;
using CalamityMod.Items;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class DeleteAllSuperDummiesPacket : CalamityPacket
    {
        public static DeleteAllSuperDummiesPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.DeleteAllSuperDummies;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            // There is nothing in the packet! We only need to call delete method on Server
            if (Main.dedServ)
                SuperDummy.DeleteDummies();
        }
    }
}
