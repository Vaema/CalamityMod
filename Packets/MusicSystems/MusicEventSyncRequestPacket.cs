using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Systems;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Packets
{
    public sealed class MusicEventSyncRequestPacket : CalamityPacket
    {
        public static MusicEventSyncRequestPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.MusicEventSyncRequest;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            // Only MP Client should send request!
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            var packet = Instance.CreateBasePacket();
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            // Only fulfill requests as the server host
            if (Main.netMode != NetmodeID.Server)
                return;

            MusicEventSyncResponsePacket.Send(toClient: sender);
        }
    }
}
