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

        public static CalamityPlayer ReadCalamityPlayer(this BinaryReader reader) => ReadPlayer(reader)?.Calamity() ?? null;
        public static Player ReadPlayer(this BinaryReader reader)
        {
            int index = reader.ReadByte();
            
            if (index >= Main.maxPlayers)
                return null;

            return Main.player[index];
        }

        public static NPCType ReadModNPC<NPCType>(this BinaryReader reader) where NPCType : ModNPC => ReadNPC(reader)?.ModNPC as NPCType;
        public static ModNPC ReadModNPC(this BinaryReader reader) => ReadNPC(reader)?.ModNPC ?? null;
        public static NPC ReadNPC(this BinaryReader reader)
        {
            int index = reader.ReadByte();

            if (index >= Main.maxNPCs)
                return null;

            return Main.npc[index];
        }
    }
}
