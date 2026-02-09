using System.IO;
using CalamityMod.Items.Tools;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class DeleteAllSuperDummiesPacket : CalamityPacket
    {
        public static DeleteAllSuperDummiesPacket Instance { get; private set; }

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            // There is nothing in the packet! We only need to call delete method on Server
            if (Main.dedServ)
                SuperDummy.DeleteDummies();
        }
    }
}
