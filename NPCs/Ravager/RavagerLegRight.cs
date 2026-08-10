using CalamityMod.Events;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.Ravager;

[HasPierceResist]
public class RavagerLegRight : ModNPC
{
    public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.RavagerBody.DisplayName");
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.NeedsExpertScaling[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.damage = 0; // No contact damage
        NPC.width = 60;
        NPC.height = 60;
        NPC.defense = 40;
        NPC.DR_NERD(0.15f);
        NPC.lifeMax = 12500;
        NPC.knockBackResist = 0f;
        AIType = -1;
        NPC.netAlways = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.alpha = 255;
        NPC.HitSound = RavagerBody.HitSound;
        NPC.DeathSound = RavagerBody.LimbLossSound;
        if (DownedBossSystem.downedProvidence && !BossRushEvent.BossRushActive)
        {
            NPC.defense *= 2;
            NPC.lifeMax *= 4;
        }
        if (BossRushEvent.BossRushActive)
        {
            NPC.lifeMax = 40000;
        }
        NPC.Calamity().VulnerableToSickness = false;
        NPC.Calamity().VulnerableToWater = true;
    }

    public override void AI()
    {
        if (CalamityGlobalNPC.scavenger < 0 || !Main.npc[CalamityGlobalNPC.scavenger].active)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.StrikeInstantKill();

            return;
        }

        if (NPC.alpha > 0)
        {
            NPC.alpha -= 10;
            if (NPC.alpha < 0)
                NPC.alpha = 0;

            NPC.ai[1] = 0f;
        }

        NPC.Center = Main.npc[CalamityGlobalNPC.scavenger].Center + new Vector2(70f, 88f);
    }

    public override bool CheckActive() => false;

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int k = 0; k < 3; k++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection, -1f, 0, default, 1f);
        }
        if (NPC.life <= 0)
        {
            if (!Main.dedServ)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ScavengerLegRight").Type, 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ScavengerLegRight2").Type, 1f);
            }
            for (int k = 0; k < 20; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
