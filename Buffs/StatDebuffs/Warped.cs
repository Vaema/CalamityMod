using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs;

public class Warped : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Calamity().warped = true;
        if (player.miscCounter % 10 == 0)
        {
            float halfWidth = player.width * 0.5f;
            float halfHeight = player.height * 0.5f;
            for (var i = 0; i < 1; i++)
            {
                var position = player.Center + new Vector2(Main.rand.NextFloat(-halfWidth, halfWidth), Main.rand.NextFloat(-halfHeight, halfHeight));
                Particle arrow = new StatChangeArrow(position, -(Vector2.UnitY * 5).RotatedByRandom(1f), -MathHelper.PiOver2, Color.Cyan, Color.Cyan * 0f, 0.75f, 60);
                ((StatChangeArrow)arrow).AffectedByGravity = true;
                GeneralParticleHandler.SpawnParticle(arrow);
            }
        }
    }
}
