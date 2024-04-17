using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class Laceration : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().laceration = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.Calamity().laceration < npc.buffTime[buffIndex])
                npc.Calamity().laceration = npc.buffTime[buffIndex];
            npc.DelBuff(buffIndex);
            buffIndex--;
        }

        internal static void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (Main.rand.NextBool(3))
            {
                for (int b = 0; b < 2; b++)
                {
                    Vector2 bloodVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                    bloodVel.SafeNormalize(Vector2.Zero);
                    bloodVel *= Main.rand.NextFloat(4f, 7f);

                    Particle bloody = new BloodParticle(npc.Center, bloodVel, 40, 0.75f, new Color(192, 0, 0));
                    GeneralParticleHandler.SpawnParticle(bloody);
                }
            }
        }
    }
}
