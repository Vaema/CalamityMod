using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.World;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class Sharkoon : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 23;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 20;
            NPC.width = 20;
            NPC.height = 58;
            NPC.defense = 5;
            NPC.lifeMax = 350;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.15f;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<SharkoonBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Sharkoon")
            });
        }

        public override void AI()
        {
            CalamityAI.PassiveSwimmingAI(NPC, Mod, 1, 1000f, 0.15f, 0.15f, 2.5f, 2.5f, 0.1f);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 6 && NPC.frame.Y < frameHeight * 22)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            // EKUSPLOSIONE
            if (NPC.ai[1] == 1)
            {
                if (NPC.frame.Y < frameHeight * 9)
                {
                    NPC.frame.Y = frameHeight * 9;
                }
            }
            // Idle
            else
            {
                if (NPC.frame.Y > 8 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return null;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.9f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedMoss, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedMoss, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }
    }
}
