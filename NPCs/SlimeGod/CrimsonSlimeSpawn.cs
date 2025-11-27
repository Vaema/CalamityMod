using System;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SlimeGod
{
    public class CrimsonSlimeSpawn : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Slime;
            NPC.damage = 20; // 40
            NPC.width = 40;
            NPC.height = 30;

            NPC.defense = 4;
            NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : 110;
            NPC.knockBackResist = 0.8f;
            AnimationType = NPCID.CorruptSlime;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = ModContent.NPCType<SplitCrimulanPaladin>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.CrimsonSlimeSpawn")
            });
        }

        public override void AI()
        {
            Vector3 light = new Vector3(0.5f, 0.1f, 0.1f);
            Lighting.AddLight(NPC.Center, light.X, light.Y, light.Z);

            NPC.damage = (NPC.velocity.Y == 0f || NPC.velocity.Length() < 3f) ? 0 : NPC.defDamage;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            Color dustColor = Color.Crimson;
            dustColor.A = 150;

            for (int k = 0; k < 5; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hit.HitDirection, -1f, 0, dustColor, 1f);

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hit.HitDirection, -1f, 0, dustColor, 1f);
            }
        }

        public override void OnKill()
        {
            int closestPlayer = Player.FindClosest(NPC.Center, 1, 1);
            if (Main.rand.NextBool(8) && Main.player[closestPlayer].statLife < Main.player[closestPlayer].statLifeMax2)
                Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);

            if (Main.zenithWorld && Main.rand.NextBool(5))
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 valueBoom = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float spreadBoom = 15f * 0.0174f;
                    double startAngleBoom = Math.Atan2(NPC.velocity.X, NPC.velocity.Y) - spreadBoom / 2;
                    double deltaAngleBoom = spreadBoom / 8f;
                    double offsetAngleBoom;
                    int iBoom;
                    int damageBoom = 30;
                    for (iBoom = 0; iBoom < 5; iBoom++)
                    {
                        int projectileType = ModContent.ProjectileType<UnstableCrimulanGlob>();
                        offsetAngleBoom = startAngleBoom + deltaAngleBoom * (iBoom + iBoom * iBoom) / 2f + 32f * iBoom;
                        float velocityfactor = 0.3f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), valueBoom.X, valueBoom.Y, (float)(Math.Sin(offsetAngleBoom) * velocityfactor), (float)(Math.Cos(offsetAngleBoom) * velocityfactor), projectileType, damageBoom, 0f, Main.myPlayer, 0f, 0f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), valueBoom.X, valueBoom.Y, (float)(-Math.Sin(offsetAngleBoom) * velocityfactor), (float)(-Math.Cos(offsetAngleBoom) * velocityfactor), projectileType, damageBoom, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
            }
        }
    }
}
