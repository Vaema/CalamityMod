using System.IO;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class SpawnBossOnPositionPacket : CalamityPacket
    {
        public static SpawnBossOnPositionPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.SpawnBossOnPosition;

        public static void Send(int x, int y, int npcType, Player target = null, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            packet.Write(x);
            packet.Write(y);
            packet.Write(npcType);
            packet.Write((byte)(target?.whoAmI ?? byte.MaxValue));
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var x = packet.ReadInt32();
            var y = packet.ReadInt32();
            var npcType = packet.ReadInt32();
            var target = packet.ReadPlayer();
            var targetIndex = target?.whoAmI ?? 255;

            if (!Main.dedServ)
                return;

            int spawnedNPCIdx = NPC.NewNPC(NPC.GetBossSpawnSource(targetIndex), x, y, npcType, Start: 1);
            if (spawnedNPCIdx >= Main.maxNPCs)
                return;

            NPC npc = Main.npc[spawnedNPCIdx];
            npc.timeLeft *= 20;
            npc.target = targetIndex;

            CalamityUtils.BossAwakenMessage(spawnedNPCIdx);
            CalamityNetcode.SyncNPC(npc);
        }
    }
}
