using System.IO;
using CalamityMod.TileEntities;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class TETurretPacket : CalamityPacket
    {
        public static TETurretPacket Instance { get; private set; }

        public static void Send(TEBaseTurret turret, int toClient = -1, int ignoreClient = -1)
        {
            if (turret is null)
                return;

            var packet = Instance.CreateBasePacket();
            packet.WriteTileEntityID(turret);
            packet.Write(turret.FiringTime);
            packet.Write(turret.Angle);
            packet.WriteVector2(turret.TargetPos);
            turret.WriteExtraTurretData(packet);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var turret = packet.ReadTileEntity<TEBaseTurret>();
            int firingTime = packet.ReadInt32();
            float angle = packet.ReadSingle();
            Vector2 targetVec = packet.ReadVector2();

            if (turret is not null)
            {
                turret.FiringTime = firingTime;
                turret.Angle = angle;
                turret.TargetPos = targetVec;
                turret.ReadExtraTurretData(packet);
            }
            else
            {
                // Otherwise, discard the fixed extra bytes so the message stream doesn't go haywire.
                _ = packet.ReadBytes(TEBaseTurret.NumExtraBytes);
            }
        }
    }
}
