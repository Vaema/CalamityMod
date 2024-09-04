using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Packets
{
    public abstract class CalamityPacket
    {
        public abstract byte MessageType { get; }
        public abstract void HandlePacket(in BinaryReader packet, int sender);

        public void CloneAndBroadcast(in BinaryReader packet, long startIndex, int length, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            if (startIndex < 0)
                return;

            packet.BaseStream.Position = startIndex;

            Span<byte> buffer = stackalloc byte[length];
            packet.BaseStream.Read(buffer);

            var newPacket = CreateBasePacket();
            newPacket.Write(buffer);
            newPacket.Send(ignoreClient);
        }

        public ModPacket CreateBasePacket()
        {
            var packet = CalamityMod.Instance.GetPacket();
            packet.Write(MessageType);
            return packet;
        }
    }
}
