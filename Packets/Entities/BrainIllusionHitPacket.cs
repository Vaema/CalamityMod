using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Projectiles.Boss.BrainOfCthulhu;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Packets.Entities;

internal class BrainIllusionHitPacket : CalamityPacket
{
    public static BrainIllusionHitPacket Instance { get; private set; }

    public static void Send(int illusionIndex, int foolIndex)
    {
        // Only Client should send to Server
        if (Main.dedServ)
            return;

        var packet = Instance.CreateBasePacket();

        packet.Write((byte)illusionIndex);
        packet.Write((byte)foolIndex);

        packet.Send();
    }

    public override void HandlePacket(BinaryReader packet, int sender)
    {
        var bytes = packet.ReadBytes(2);
        NPC illusion = Main.npc[(int)bytes[0]];
        Player fool = Main.player[(int)bytes[1]];
        if(Main.dedServ)
            Projectile.NewProjectile(illusion.GetSource_FromThis(), illusion.Center, Vector2.Zero, ModContent.ProjectileType<TelekineticBlast>(), 50, 0.5f, -1, fool.whoAmI, 5, illusion.whoAmI);
        illusion.dontTakeDamage = true;
        illusion.netUpdate = true;
    }
}
