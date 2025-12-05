using System.IO;
using CalamityMod.World;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class CodebreakerSummonStuffPacket : CalamityPacket
    {
        public static CodebreakerSummonStuffPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.CodebreakerSummonStuff;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(CalamityWorld.DraedonSummonCountdown);
            packet.WriteVector2(CalamityWorld.DraedonSummonPosition);
            packet.Write(CalamityWorld.DraedonMechdusa);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            CalamityWorld.DraedonSummonCountdown = packet.ReadInt32();
            CalamityWorld.DraedonSummonPosition = packet.ReadVector2();
            CalamityWorld.DraedonMechdusa = packet.ReadBoolean();
        }
    }
}
