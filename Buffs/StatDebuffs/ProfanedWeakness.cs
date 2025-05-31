using CalamityMod.Dusts;
using CalamityMod.Particles;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class ProfanedWeakness : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().relicOfResilienceWeakness = true;
        }
        internal static void DrawEffects(NPC npc, ref Color drawColor)
        {
            Vector2 npcSize = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, npc.width / 2), Main.rand.NextFloat(-npc.height / 2, npc.height / 2));

            if (Main.rand.NextBool(5))
            {
                Particle spark = new CustomSpark(npcSize, Vector2.Zero, "CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard" + Main.rand.Next(1, 6 + 1).ToString(), true, Main.rand.Next(9, 22 + 1), Main.rand.NextFloat(0.4f, 0.8f), Color.White * Main.rand.NextFloat(0.4f, 0.9f), new Vector2(1.1f, 0.8f), false, false, Main.rand.NextFloat(-5, 5), false, false);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(npc.position - new Vector2(2f), npc.width + 4, npc.height + 4, ModContent.DustType<LightDust>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, Main.rand.NextFloat(0.9f, 1.7f));
                dust.noGravity = Main.rand.NextBool(3);
                dust.color = Main.rand.NextBool() ? Color.Sienna : Color.Goldenrod;
            }
        }
    }
}
