using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.TileEntities;

public static class TileEntityTimeHandler
{
    private static int _factoryType = -1;
    private static int _chargerType = -1;
    private static int _codebreakerType = -1;

    public static void Update()
    {
        MultiplayerClientUpdateVisuals();
    }

    private static void MultiplayerClientUpdateVisuals()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        if (_factoryType == -1) _factoryType = ModContent.GetInstance<TEPowerCellFactory>().Type;
        if (_chargerType == -1) _chargerType = ModContent.GetInstance<TEChargingStation>().Type;
        if (_codebreakerType == -1) _codebreakerType = ModContent.GetInstance<TECodebreaker>().Type;

        // Iterate the tile entities collection without creating extra allocations.
        foreach (TileEntity te in TileEntity.ByID.Values)
        {
            if (te == null)
                continue;

            if (te.type == _factoryType)
            {
                TEPowerCellFactory factory = (TEPowerCellFactory)te;
                ++factory.Time;
            }
            else if (te.type == _chargerType)
            {
                TEChargingStation charger = (TEChargingStation)te;

                if (charger.ClientChargingDust && charger.CanDoWork)
                {
                    charger.ClientChargingDust = false;
                    charger.SpawnChargingDust();
                }
            }
            else if (te is TEBaseTurret turret)
            {
                turret.UpdateClient();
                turret.UpdateAngle();
            }
            else if (te.type == _codebreakerType)
            {
                TECodebreaker codebreaker = (TECodebreaker)te;
                codebreaker.UpdateTime();
            }
        }
    }
}
