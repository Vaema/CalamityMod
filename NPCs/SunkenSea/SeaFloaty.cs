using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class SeaFloaty : SunkenSeaNPC
    {
        private bool hasBeenHit = false;

        protected override List<int> PreyIDs => new List<int>();

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<Polyperil>(),
            ModContent.NPCType<PolyperilTentacle>(),
            ModContent.NPCType<Sharkoon>(),
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = -1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            NPCID.Sets.CountsAsCritter[Type] = true;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.npcSlots = 0.5f;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 44;
            NPC.height = 22;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.knockBackResist = 0.5f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.chaseable = false;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<SeaFloatyBanner>();
            NPC.catchItem = ModContent.ItemType<SeaFloatyItem>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.SeaFloaty")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
            writer.Write(hasBeenHit);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
            hasBeenHit = reader.ReadBoolean();
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            if (NPC.velocity.X > 0.25f)
            {
                NPC.spriteDirection = 1;
            }
            else if (NPC.velocity.X < 0.25f)
            {
                NPC.spriteDirection = -1;
            }
            if (NPC.ai[0] == 0f)
            {
                NPC.direction = -1;
                NPC.ai[0] = 1f;
            }
            NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * 0.1f;
            if (NPC.velocity.X < -2.5f || NPC.velocity.X > 2.5f)
            {
                NPC.velocity.X = NPC.velocity.X * 0.95f;
            }
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.velocity.X * -1f;
                NPC.direction *= -1;
                NPC.netUpdate = true;
            }
            // panic when it gets hit or the player is close enough to it
            if ((NPC.justHit || CurrentPlayer != null || CurrentPredator != null) && !hasBeenHit)
            {
                hasBeenHit = true;
                NPC.noTileCollide = true;
                NPC.noGravity = true;

                SoundEngine.PlaySound(SoundID.NPCHit37 with { Pitch = 1 }, NPC.Center);

                if (!Main.dedServ)
                {
                    var emoteDirection = -Vector2.UnitY * Main.rand.NextFloat(2f, 3f);
                    Particle emote = new EmoteExpressionParticle(
                        NPC.Center + emoteDirection * 2f,
                        emoteDirection,
                        2.2f,
                        Color.Yellow,
                        Main.rand.Next(30, 46),
                        EmoteExpressionParticle.EmoteType.Exclamation);
                    GeneralParticleHandler.SpawnParticle(emote);
                }
            }
            NPC.chaseable = hasBeenHit;
            if (hasBeenHit)
            {
                NPC.TargetClosest(true);
                NPC.velocity.X = NPC.velocity.X - (float)NPC.direction * 0.5f;
                NPC.velocity.Y = NPC.velocity.Y - (float)NPC.directionY * 0.3f;
                if (NPC.velocity.X > 10f)
                {
                    NPC.velocity.X = 10f;
                }
                if (NPC.velocity.X < -10f)
                {
                    NPC.velocity.X = -10f;
                }
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
                if (NPC.velocity.Y < -10f)
                {
                    NPC.velocity.Y = -10f;
                }
                NPC.direction *= -1;
                NPC.rotation = NPC.velocity.X * 0.1f;
                if ((double)NPC.rotation < -0.3)
                {
                    NPC.rotation = -0.3f;
                }
                if ((double)NPC.rotation > 0.3)
                {
                    NPC.rotation = 0.3f;
                    return;
                }
            }
        }
        protected override bool NPCSearchFilter(NPC n)
        {
            return NPC.HasSight(n.Center) && Vector2.DistanceSquared(NPC.Center, n.Center) < 360f * 360f && PredatorIDs.Contains(n.type);
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return NPC.HasSight(p.Center) && Vector2.DistanceSquared(NPC.Center, p.Center) < 360f * 360f;
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += hasBeenHit ? 0.3f : 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneRadiantReefs && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.45f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
            CalamityUtils.SpawnGores(NPC, "SeaFloaty", 1);
        }
    }
}
