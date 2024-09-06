using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.Enums;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {
        public static void WriteWhoAmI(this BinaryWriter writer, Entity entity)
        {
            if (entity is NPC npc)
            {
                writer.Write((byte)npc.whoAmI);
            }
            else if (entity is Player player)
            {
                writer.Write((byte)player.whoAmI);
            }
            else
            {
                CalamityMod.Instance.Logger.Error($"Type: {entity} is not eligible for networking! We'll still send whoAmI for packet align, but we should fix this immediately! {Environment.StackTrace}");
                writer.Write((byte)entity.whoAmI);
            }
        }

        public static CalamityPlayer ReadCalamityPlayer(this BinaryReader reader, bool nullOnInactive = true) => ReadPlayer(reader, nullOnInactive)?.Calamity() ?? null;
        public static Player ReadPlayer(this BinaryReader reader, bool nullOnInactive = true)
        {
            int index = reader.ReadByte();
            
            if (index >= Main.maxPlayers)
                return null;

            var player = Main.player[index];

            if (nullOnInactive && player.IsNullOrInactive())
                return null;

            return player;
        }

        public static NPCType ReadModNPC<NPCType>(this BinaryReader reader, bool nullOnInactive = true) where NPCType : ModNPC => ReadNPC(reader, nullOnInactive)?.ModNPC as NPCType;
        public static ModNPC ReadModNPC(this BinaryReader reader, bool nullOnInactive = true) => ReadNPC(reader, nullOnInactive)?.ModNPC ?? null;
        public static NPC ReadNPC(this BinaryReader reader, bool nullOnInactive = true)
        {
            int index = reader.ReadByte();

            if (index >= Main.maxNPCs)
                return null;

            var npc = Main.npc[index];

            if (nullOnInactive && npc.IsNullOrInactive())
                return null;

            return npc;
        }
    }
}
