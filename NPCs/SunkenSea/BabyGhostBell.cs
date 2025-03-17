using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Fishing.SunkenSeaCatches;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class BabyGhostBell : ModNPC
    {
        public bool hasBeenHit = false;
        public static Texture2D RadiantTexture;
        public static Texture2D VoltaicTexture;
        public static Texture2D RedTexture;
        public static Texture2D GreenTexture;
        public static Texture2D GoldTexture;
        public ref float Variant => ref NPC.ai[1];
        public enum JellyColor
        {
            Blue = 0,
            Green = 1,
            Red = 2,
            Radiant = 3,
            Voltaic = 4,
            Gold = 5
        }

        public static Asset<Texture2D> GlowTexture;

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                RadiantTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellRadiant", AssetRequestMode.ImmediateLoad).Value;
                VoltaicTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellVoltaic", AssetRequestMode.ImmediateLoad).Value;
                GreenTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellGreen", AssetRequestMode.ImmediateLoad).Value;
                RedTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellRed", AssetRequestMode.ImmediateLoad).Value;
                GoldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellGold", AssetRequestMode.ImmediateLoad).Value;
            }
            Main.npcFrameCount[Type] = 6;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.chaseable = false;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 28;
            NPC.height = 36;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.knockBackResist = 1f;
            NPC.alpha = 100;
            NPC.HitSound = SoundID.NPCHit25;
            NPC.DeathSound = SoundID.NPCDeath28;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<BabyGhostBellBanner>();
            NPC.catchItem = (short)ModContent.ItemType<BabyGhostBellItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.BabyGhostBell")
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

        public override void OnSpawn(IEntitySource source)
        {
            // Bells released by the player do not randomize when spawned
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
            Variant = Main.rand.Next(1, 4);
            if (Main.rand.NextBool(30))
            {
                Variant = (int)JellyColor.Radiant;
            }
            if (Main.rand.NextBool(15))
            {
                Variant = (int)JellyColor.Voltaic;
            }
            if (Main.rand.NextBool(50))
            {
                Variant = (int)JellyColor.Gold;
            }
            switch (Variant)
            {
                case (int)JellyColor.Blue:
                    NPC.catchItem = ModContent.ItemType<BabyGhostBellItem>();
                    break;
                case (int)JellyColor.Green:
                    NPC.catchItem = ModContent.ItemType<BabyGhostBellGreenItem>();
                    break;
                case (int)JellyColor.Red:
                    NPC.catchItem = ModContent.ItemType<BabyGhostBellRedItem>();
                    break;
                case (int)JellyColor.Radiant:
                    {
                        NPC.catchItem = ModContent.ItemType<BabyGhostBellRadiantItem>();
                        NPC.rarity = 3;
                    }
                    break;
                case (int)JellyColor.Gold:
                    {
                        NPC.catchItem = ModContent.ItemType<BabyGhostBellGoldItem>();
                        NPC.rarity = 3;
                    }
                    break;
                case (int)JellyColor.Voltaic:
                    {
                        NPC.catchItem = ModContent.ItemType<VoltaicJelly>();
                        NPC.rarity = 3;
                    }
                    break;
            }
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Main.rand.NextBool(20))
                    NPC.catchItem = (short)ModContent.ItemType<RustedJingleBell>();
                NPC.localAI[0] = 1f;
                NPC.velocity.Y = -3f;
                NPC.netUpdate = true;
            }
            if (Main.rand.Next(8) < 1 && NPC.catchItem == (short)ModContent.ItemType<RustedJingleBell>())
            {
                int dust = Dust.NewDust(NPC.position - new Vector2(2f, 2f), NPC.width + 4, NPC.height + 4, DustID.BlueCrystalShard, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f, 200, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.1f;
                Main.dust[dust].velocity.Y += 0.25f;
                Main.dust[dust].noLight = true;
                if (Main.rand.NextBool())
                {
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].scale *= 0.5f;
                }
            }
            // Quick lil hops in random directions for movement
            if (NPC.wet)
            {
                NPC.height = (int)(36 * NPC.scale);
                if (NPC.velocity.Length() < 1f)
                {
                    NPC.velocity = Main.rand.NextVector2CircularEdge(8, 8);
                }
                NPC.velocity *= 0.95f;
                NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.5f);
            }
            else
            {
                // Height is changed so that the jelly looks like it's actually laying on the ground when rotated
                NPC.height = (int)(24 * NPC.scale);
                // Gravy
                if (NPC.velocity.Y < 10)
                {
                    NPC.velocity.Y += 0.5f;
                }
                // Once it has hit the ground, fall over
                if (Math.Abs(NPC.velocity.Y) < 1 && NPC.collideY)
                {
                    // Splat
                    if (NPC.ai[2] == 0)
                    {
                        SoundEngine.PlaySound(CnidarianJellyfishOnTheString.SlapSound, NPC.Center);
                        NPC.ai[2] = 1;
                    }
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.PiOver2, 0.7f);
                }
                // Reset the splat sound
                else
                {
                    NPC.ai[2] = 0;
                }
            }
            // Lite
            Color lightColor = Color.LightBlue; // Voltaic uses the same light blue
            switch (Variant)
            {
                case (int)JellyColor.Red:
                    lightColor = Color.Pink;
                    break;
                case (int)JellyColor.Green:
                    lightColor = Color.LightGreen;
                    break;
                case (int)JellyColor.Radiant:
                    lightColor = Color.LightBlue * 1.1f; // Radiant glows brighter
                    break;
            }
            Lighting.AddLight(NPC.Center, (lightColor.R - NPC.alpha) * 1f / 255f, (lightColor.G - NPC.alpha) * 1f / 255f, (lightColor.B - NPC.alpha) * 1f / 255f);

            if (Variant == (int)JellyColor.Gold)
            {
                NPC.ProduceGoldCritterDust();
                NPC.rarity = 3;
            }
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return hasBeenHit;
            }
            return null;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 1.5f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dustType = DustID.BlueCrystalShard;
            switch (Variant)
            {
                case (int)JellyColor.Red:
                    dustType = DustID.RedTorch;
                    break;
                case (int)JellyColor.Green:
                    dustType = DustID.GemEmerald;
                    break;
                case (int)JellyColor.Gold:
                    dustType = DustID.GoldCritter;
                    break;
            }
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[Type].Value;
            switch (Variant)
            {
                case (int)JellyColor.Red:
                    texture = RedTexture;
                    break;
                case (int)JellyColor.Green:
                    texture = GreenTexture;
                    break;
                case (int)JellyColor.Radiant:
                    texture = RadiantTexture;
                    break;
                case (int)JellyColor.Voltaic:
                    texture = VoltaicTexture;
                    break;
                case (int)JellyColor.Gold:
                    texture = GoldTexture;
                    break;
            }
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / Main.npcFrameCount[Type] / 2);
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, npcOffset, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }
}
