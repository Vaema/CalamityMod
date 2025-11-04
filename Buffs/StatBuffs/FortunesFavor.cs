using CalamityMod.DataStructures;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatBuffs
{
    public class FortunesFavor : ModBuff
    {
        public override string Texture => "CalamityMod/Buffs/StatBuffs/AbsorberRegen"; // PLACEHOLDER!!!
        public static int FortunesFavorRegenBoost = 3;
        public override LocalizedText Description => base.Description.WithFormatArgs(FortunesFavorRegenBoost.ToRegenPerSecond());
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().fortunesFavor = true; // Draw FX handled in CalamityPlayerDrawEffects as is standard for regen buffs
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().fortunesFavor = true;
        }

        internal static void DrawEffects(NPC npc, ref Color drawColor)
        {
            Vector2 npcSize = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, npc.width / 2), Main.rand.NextFloat(-npc.height / 2, npc.height / 2));

            if (Main.rand.NextBool(16))
            {
                Particle Plus = new HealingPlus(npcSize, Main.rand.NextFloat(0.3f, 0.5f), new Vector2(0, Main.rand.NextFloat(-2f, -3.5f)) + npc.velocity, Color.Gold, Color.Goldenrod, Main.rand.Next(9, 13));
                GeneralParticleHandler.SpawnParticle(Plus);
            }
            Lighting.AddLight(npc.Center, Color.Gold.ToVector3() * 0.1f);
        }

    }
}
