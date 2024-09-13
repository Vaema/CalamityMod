using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.World;
using Terraria;

namespace CalamityMod.Packets
{
    public sealed class ScammedByTinkererPacket : CalamityPacket
    {
        public static ScammedByTinkererPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.ScammedByTinkerer;

        public static void Send(int amountStolenByBandit, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write7BitEncodedInt(amountStolenByBandit);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var amountStolenByBandit = packet.Read7BitEncodedInt();

            CalamityWorld.MoneyStolenByBandit += amountStolenByBandit;
            CalamityWorld.Reforges++;

            // Broadcast back for tragic event
            // WorldSync DO sync the MoneyStolenByBandit and Refores variable, But spamming SyncWorld is not a ideal action
            if (Main.dedServ)
                Send(amountStolenByBandit, ignoreClient: sender);
        }
    }
}
