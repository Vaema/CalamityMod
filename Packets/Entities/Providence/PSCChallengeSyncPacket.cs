using System.IO;
using CalamityMod.NPCs.Providence;

namespace CalamityMod.Packets
{
    internal sealed class PSCChallengeSyncPacket : CalamityPacket
    {
        public static PSCChallengeSyncPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.PSCChallengeSync;

        public static void Send(Providence providence, int toClient = -1, int ignoreClient = -1)
        {
            if (providence is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(providence);
            packet.Write(providence.challenge);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var providence = packet.ReadModNPC<Providence>();
            var challenge = packet.ReadBoolean();

            if (providence is null)
                return;

            providence.challenge = challenge;
        }
    }
}
