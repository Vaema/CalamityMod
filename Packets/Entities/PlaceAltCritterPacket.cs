using System.IO;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class PlaceAltCritterPacket : CalamityPacket
    {
        public static PlaceAltCritterPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.PlaceAltCritter;

        /// <summary>
        /// Same Method, but uses critterItem.makeNPC and critterItem.type for shorthanded call
        /// </summary>
        public static void Send(Player placer, int x, int y, Item critterItem, int colorType, int toClient = -1, int ignoreClient = -1)
        {
            if (critterItem is null)
                return;

            Send(placer, x, y, critterItem.makeNPC, critterItem.type, colorType, toClient, ignoreClient);
        }

        public static void Send(Player placer, int x, int y, int critterNPCType, int itemType, int colorType, int toClient = -1, int ignoreClient = -1)
        {
            if (placer is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteWhoAmI(placer);
            packet.Write(x);
            packet.Write(y);
            packet.Write(critterNPCType);
            packet.Write(itemType);
            packet.Write(colorType);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var placerplayer = packet.ReadPlayer();
            int posX = packet.ReadInt32();
            int posY = packet.ReadInt32();
            int type = packet.ReadInt32();
            int itemType = packet.ReadInt32();
            float color = packet.ReadInt32();
            if (Main.dedServ && placerplayer is not null)
            {
                int newNPCIndex = NPC.NewNPC(placerplayer.GetSource_ReleaseEntity(), posX, posY, type, ai1: color);
                if (newNPCIndex >= Main.maxNPCs)
                    return;

                var npc = Main.npc[newNPCIndex];
                npc.catchItem = itemType;
                npc.releaseOwner = (short)placerplayer.whoAmI;
                CalamityNetcode.SyncNPC(npc); // releaseOwner should be synced in here
            }
        }
    }
}
