using System.IO;
using System.Linq;
using CalamityMod.Systems;
using CalamityMod.UI.ModeIndicator;
using Terraria;

namespace CalamityMod.Packets
{
    internal sealed class SwitchToDifficultyPacket : CalamityPacket
    {
        public static SwitchToDifficultyPacket Instance { get; private set; }

        public static void Send(DifficultyMode modeToSwitch, int toClient = -1, int ignoreClient = -1)
        {
            var packet = Instance.CreateBasePacket();
            // Simple Solution: Just write FullName of the DifficultyMode type to avoid any desync
            // This is tolerable as this packet is designed to be not be called frequently
            packet.Write(modeToSwitch.FullName);
            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            var modeName = packet.ReadString();
            var difficulty = DifficultyModeSystem.Difficulties.SingleOrDefault(diff => diff.FullName.Equals(modeName));

            if (difficulty != null)
            {
                ModeIndicatorUI.SwitchToDifficulty(difficulty, broadcast: false);

                if (Main.dedServ)
                    Send(difficulty, ignoreClient: sender);
            }
            else
            {
                CalamityMod.Log.Error($"Packet: [{nameof(SwitchToDifficultyPacket)}] has failed! Name: [{modeName}] is not a valid {nameof(DifficultyMode)} name!");
            }
        }
    }
}
