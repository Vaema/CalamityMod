using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.World;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class UnlockAbyssChestsPacket : CalamityPacket
    {
        public static UnlockAbyssChestsPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.UnlockAbyssChests;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            Abyss.DoUnlockAllAbyssChests();
        }
    }
}
