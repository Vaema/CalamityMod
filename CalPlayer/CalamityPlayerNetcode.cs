using System.Collections.Generic;
using System.Linq;
using CalamityMod.Cooldowns;
using CalamityMod.Packets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer;

public partial class CalamityPlayer : ModPlayer
{
    #region Standard Syncs
    internal const int GlobalSyncPacketTimer = 15;

    private void EnterWorldSync()
    {
        StandardSync();
    }

    internal void StandardSync()
    {
        RageSyncPacket.Send(this);
        AdrenalineSyncPacket.Send(this);
        DefenseDamageSyncPacket.Send(this);
    }

    internal void MousePositionSync()
    {
        MousePositionSyncPacket.Send(this);
    }

    internal void MouseRotationSync()
    {
        MouseRotationSyncPacket.Send(this);
    }

    internal void MouseRightClickSync()
    {
        RightClickSyncPacket.Send(this);
    }
    #endregion

    #region Creating and Sending Packets
    public void SyncCooldownAddition(bool server, CooldownInstance cd)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        CooldownAdditionPacket.Send(this, cd);
    }

    public void SyncCooldownRemoval(bool server, IList<string> cooldownIDs)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        CooldownRemovalPacket.Send(this, [.. cooldownIDs.Select(id => CooldownRegistry.Get(id).netID)]);
    }

    public void SyncCooldownDictionary(bool server)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        SyncCooldownDictionaryPacket.Send(this);
    }

    #endregion
}
