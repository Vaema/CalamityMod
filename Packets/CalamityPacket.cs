using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Packets
{
    public abstract class CalamityPacket
    {
        public abstract byte MessageType { get; }
        public abstract void HandlePacket(in BinaryReader packet, int sender);

        public virtual void OnLoaded() { }
        public virtual void OnUnloaded() { }

        internal PropertyInfo _Prop_Static_Instance;

        public void CloneAndBroadcast(in BinaryReader packet, long startIndex, int length, int ignoreClient = -1)
        {
            if (!Main.dedServ)
                return;

            if (startIndex < 0)
                return;

            packet.BaseStream.Position = startIndex;

            // Limit stackalloc size to 256 bytes
            Span<byte> buffer = length <= 256 ? stackalloc byte[length] : new byte[length];
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
