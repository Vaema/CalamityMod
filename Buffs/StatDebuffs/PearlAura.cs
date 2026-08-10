using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs;

public class PearlAura : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.Calamity().pearlAura = true;
    }

    internal static void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (Main.rand.NextBool(4))
        {
            int dustType = Main.rand.NextBool() ? DustID.GemSapphire : DustID.BlueCrystalShard;
            Vector2 dustVelocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 12f, 0f)) * (Main.rand.NextBool() ? -1f : 1f);
            Dust theDust = Dust.NewDustDirect(npc.position, npc.width, npc.height, dustType, dustVelocity.X, dustVelocity.Y);
            theDust.noGravity = true;
        }
    }
}
