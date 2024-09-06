using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.Enums;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {
        #region TileEntity RW
        public static void WriteTileEntityID(this BinaryWriter writer, TileEntity tileEntity)
        {
            if (tileEntity is null)
            {
                writer.Write(int.MaxValue);
                return;
            }

            if (!TileEntity.ByID.ContainsKey(tileEntity.ID))
            {
                writer.Write(int.MaxValue);
                return;
            }

            writer.Write(tileEntity.ID);
        }

        public static TileEntityType ReadTileEntity<TileEntityType>(this BinaryReader reader) where TileEntityType : TileEntity
            => ReadTileEntity(reader) as TileEntityType;

        public static TileEntity ReadTileEntity(this BinaryReader reader)
        {
            var id = reader.ReadInt32();
            bool exists = TileEntity.ByID.TryGetValue(id, out TileEntity tileEntity);

            return exists ? tileEntity : null;
        }
        #endregion TileEntity RW

        #region Entity RW
        public static void WriteWhoAmI(this BinaryWriter writer, ModPlayer player) => WriteWhoAmI(writer, player?.Player);
        public static void WriteWhoAmI(this BinaryWriter writer, Player player)
        {
            byte whoAmI = (byte)(player?.whoAmI ?? Main.maxPlayers);
            writer.Write(whoAmI);
        }

        public static void WriteWhoAmI(this BinaryWriter writer, ModNPC npc) => WriteWhoAmI(writer, npc?.NPC);
        public static void WriteWhoAmI(this BinaryWriter writer, NPC npc)
        {
            byte whoAmI = (byte)(npc?.whoAmI ?? Main.maxNPCs);
            writer.Write(whoAmI);
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
        #endregion Entity RW
    }
}
