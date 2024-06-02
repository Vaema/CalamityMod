using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class LavaSlimeNoLavaDrop : ModNPC
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.LavaSlime}";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
            this.HideFromBestiary();
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Slime;
            AIType = NPCID.LavaSlime;
            AnimationType = NPCID.LavaSlime;
            NPC.width = 24;
            NPC.height = 18;
            NPC.damage = 15;
            NPC.defense = 10;
            NPC.lifeMax = 50;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.scale = 1.1f;
            NPC.alpha = 50;
            NPC.lavaImmune = true;
            NPC.value = 120f;
            if (Main.remixWorld)
            {
                NPC.damage = 7;
                NPC.defense = 2;
                NPC.lifeMax = 25;
                NPC.value = 25f;
            }
            //Banner = NPCID.LavaSlime;
            //BannerItem = ItemID.LavaSlimeBanner;

            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToWater = true;

            // Scale stats in Expert and Master
            CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; (double)i < hit.Damage / (double)NPC.lifeMax * 80D; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, hit.HitDirection * 2, -1f, NPC.alpha, default, 1.5f);
                    if (Main.rand.Next(8) != 0)
                        Main.dust[dust].noGravity = true;
                }

                return;
            }

            for (int i = 0; i < 40; i++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 6, hit.HitDirection * 2, -1f, NPC.alpha, default, 1.5f);
                if (Main.rand.Next(8) != 0)
                    Main.dust[dust].noGravity = true;
            }
        }
    }
}
