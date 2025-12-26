using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Systems;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.Net;
using static CalamityMod.UI.DialogueDisplay.DialogueDisplaySystem;

namespace CalamityMod.Packets
{
    internal sealed class StartDialogueDisplayPacket : CalamityPacket
    {
        public static StartDialogueDisplayPacket Instance { get; private set; }

        public override byte MessageType => (byte)CalamityModMessageType.DialogueDisplayStart;

        public static void Send(string name, bool progressDialogue, Vector2 position, int index, int uptime, DisplayEffectID effect, float wrapWidth, int toClient = -1, int ignoreClient = -1)
        {
            // Only Server should send Reponse to Clients
            if (!Main.dedServ)
                return;

            var packet = Instance.CreateBasePacket();

            packet.Write(name);
            packet.WriteFlags(progressDialogue, false);
            packet.WritePackedVector2(position);
            packet.Write(index);
            packet.Write(uptime);
            packet.Write((byte)effect);
            packet.Write(wrapWidth);

            packet.Send(toClient, ignoreClient);
        }

        public enum EntityType
        {
            NPC,
            Player,
            Projectile
        }

        public static void Send(string name, bool progressDialogue, EntityType type, int entity, int index, int uptime, DisplayEffectID effect, float wrapWidth, int toClient = -1, int ignoreClient = -1)
        {
            // Only Server should send Reponse to Clients
            if (!Main.dedServ)
                return;

            var packet = Instance.CreateBasePacket();

            packet.Write(name);
            packet.WriteFlags(progressDialogue, true);
            packet.WriteFlags(type == EntityType.NPC, type == EntityType.Player);
            packet.Write(entity);
            packet.Write(index);
            packet.Write(uptime);
            packet.Write((byte)effect);
            packet.Write(wrapWidth);

            packet.Send(toClient, ignoreClient);
        }

        public override void HandlePacket(in BinaryReader packet, int sender)
        {
            // Only receive info as clients
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                packet.ReadString();
                packet.ReadFlags(out _, out bool n);

                if (n)
                {
                    packet.ReadFlags(out _, out _);
                    packet.ReadInt32();
                }
                else
                    packet.ReadPackedVector2();

                packet.ReadInt32();
                packet.ReadInt32();
                packet.ReadByte();
                packet.ReadSingle();
                return;
            }

            string name = packet.ReadString();
            packet.ReadFlags(out bool progressDialogue, out bool hasEntity);

            int entity = -1;
            Vector2 pos = Vector2.zeroVector;
            bool isNpc = false;
            bool isPlayer = false;
            if (hasEntity)
            {
                packet.ReadFlags(out isNpc, out isPlayer);
                entity = packet.ReadInt32();
            }
            else
                pos = packet.ReadPackedVector2();

            int index = packet.ReadInt32();
            int uptime = packet.ReadInt32();
            byte effect = packet.ReadByte();
            float wrapWidth = packet.ReadSingle();

            DisplayEffect de = GetEffect((DisplayEffectID)effect);

            if (hasEntity)
                StartDialogueOnClient(name, isNpc ? Main.npc[entity] : isPlayer ? Main.player[entity] : Main.projectile.FirstOrDefault(p => p.identity == entity), index, uptime, progressDialogue, de, wrapWidth);
            else
                StartDialogueOnClient(name, pos, index, uptime, progressDialogue, de, wrapWidth);

        }
    }

}
