using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;
using CalamityMod.Particles;
using System.Collections.Generic;

namespace CalamityMod.NPCs.SunkenSea
{
    public class FesteringRemains : ModNPC
    {
        public List<Particle> bones = new List<Particle>();
        public override void SetDefaults()
        {
            NPC.width = 52;
            NPC.height = 36;
            NPC.defense = 0;
            NPC.lifeMax = 200;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<AlphaSeaMinnowBanner>();
            NPC.chaseable = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.HauntedChum")
            });
        }
        public override void OnSpawn(IEntitySource source)
        {
            // Spawn some bones around the remains
            int boneCount = Main.rand.Next(5, 8);
            for (int i = 0; i < boneCount; i++)
            {
                int extraDistX = 120;
                int extraDistY = 40;
                Vector2 bonePos = NPC.Center + new Vector2(Main.rand.Next(-extraDistX, extraDistX + 1), Main.rand.Next(-extraDistY, extraDistY + 1));
                // If the bone spawns inside a tile, try to make it not spawn in a tile
                for (int j = 0; j < 50; j++)
                {
                    Tile tilePos = CalamityUtils.ParanoidTileRetrieval((int)bonePos.X / 16, (int)bonePos.Y / 16);
                    if (tilePos.HasTile)
                    {
                        bonePos = NPC.Center + new Vector2(Main.rand.Next(-extraDistX, extraDistX + 1), Main.rand.Next(-extraDistY, extraDistY + 1));
                    }
                }
                Particle bone = new ChumBone(bonePos, Vector2.Zero, NPC.GetAlpha(Color.White), Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4), Main.rand.NextFloat(0.8f, 1.6f), 4, Main.rand.NextBool(), Main.rand.NextBool());
                GeneralParticleHandler.SpawnParticle(bone);
                // Add to a list of bones
                bones.Add(bone);
            }
            NPC.TargetClosest();
        }

        public override void AI()
        {
            // Enables expert scaling 
            NPC.damage = 0;
            NPC.TargetClosest();
            int aggroRange = 80;
            int spawnTime = 60;
            Player target = Main.player[NPC.target];
            if (NPC.ai[1] == 0)
            {
                if (target != null && target.active && target.Distance(NPC.Center) < aggroRange)
                {
                    NPC.ai[0]++;
                    if (NPC.ai[0] > spawnTime)
                    {
                        int head = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y - 48, ModContent.NPCType<HauntedChum>(), ai3: NPC.whoAmI);
                        NPC.ai[1] = 1;
                        NPC.ai[2] = head;
                        Main.npc[head].localAI[0] = -MathHelper.PiOver4;
                    }
                }
            }
            NPC spawnedHead = Main.npc[(int)NPC.ai[2]];
            // If the head has already been spawned but there is no more head (no head?) start a countdown, once the countdown reaches full, the head respawns
            if (NPC.ai[1] == 1)
            {
                if (spawnedHead == null || !spawnedHead.active || spawnedHead.type != ModContent.NPCType<HauntedChum>())
                {
                    if (NPC.ai[3] == 180)
                    {
                        // Spawn the head and reset ai3 back to 0
                        int head = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y - 48, ModContent.NPCType<HauntedChum>(), ai3: NPC.whoAmI);
                        NPC.ai[2] = head;
                        NPC.ai[3] = 0;
                        Main.npc[head].localAI[0] = -MathHelper.PiOver4;
                    }
                    NPC.ai[3]++;
                }
            }
            // Keep the bones alive
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].Time = 2;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSeaShores && !spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
