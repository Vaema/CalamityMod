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
    public sealed class SyncDifficultiesPacket : CalamityPacket
    {
        // MIGRATED COMMENTS FROM: 'CalamityNetcode.cs'
        //TODO - Something so that other mods that hijack the difficulty ui can also use the remainder of the reader to have their own shit

        public static SyncDifficultiesPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.SyncDifficulties;

        public static void Send(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            var difficultyFlags = new BitsByte();
            difficultyFlags[0] = CalamityWorld.revenge;
            difficultyFlags[1] = CalamityWorld.death;
            packet.Write((byte)difficultyFlags);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var difficultyFlags = (BitsByte)packet.ReadByte();
            CalamityWorld.revenge = difficultyFlags[0];
            CalamityWorld.death = difficultyFlags[1];

            if (Main.dedServ)
                Send(ignoreClient: sender);
        }
    }
}
