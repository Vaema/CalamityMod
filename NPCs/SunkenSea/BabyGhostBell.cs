using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Enums;
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
    public class BabyGhostBell : SunkenSeaNPC
    {
        public bool hasBeenHit = false;
        public static Asset<Texture2D> RadiantTexture;
        public static Asset<Texture2D> VoltaicTexture;
        public static Asset<Texture2D> PinkTexture;
        public static Asset<Texture2D> GreenTexture;
        public static Asset<Texture2D> GoldTexture;
        public ref float Variant => ref NPC.ai[1];
        public enum JellyColor
        {
            Blue = 0,
            Green = 1,
            Pink = 2,
            Radiant = 3,
            Voltaic = 4,
            Gold = 5
        }

        /// <summary>
        /// The squish of this NPC while drawing.
        /// </summary>
        private Vector2 ScaleSquish = Vector2.One;

        // keeps track of the number of hops between a flip
        public int flipCounter;

        protected override List<int> PreyIDs => new List<int>();

        protected override List<int> PredatorIDs => new List<int>();

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.GleamingBurrows;

        public override void Load()
        {
            RadiantTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellRadiant");
            VoltaicTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellVoltaic");
            GreenTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellGreen");
            PinkTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellPink");
            GoldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/BabyGhostBellGold");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.chaseable = false;
            NPC.dontTakeDamageFromHostiles = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 28;
            NPC.height = 36;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.knockBackResist = 1f;
            NPC.alpha = 75;
            NPC.HitSound = SoundID.NPCHit25;
            NPC.DeathSound = SoundID.NPCDeath28;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<BabyGhostBellBanner>();
            NPC.catchItem = (short)ModContent.ItemType<BabyGhostBellItem>();
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
            Variant = Main.rand.Next(0, 3);
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
                case (int)JellyColor.Pink:
                    NPC.catchItem = ModContent.ItemType<BabyGhostBellPinkItem>();
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

            // Reset any squish that is done to the Ghost Bells, and clamps its upper limit to prevent it from becoming too tall
            if (ScaleSquish.Y > 1f)
                ScaleSquish.Y = MathHelper.Clamp(ScaleSquish.Y, 1f, 1.5f);
            ScaleSquish.Y = Math.Max(1f, ScaleSquish.Y - 0.025f);

            // Quick lil hops in semi-random directions for movement. its thursday
            if (NPC.wet)
            {
                NPC.height = (int)(36 * NPC.scale);
                if (NPC.velocity.Length() < 0.5f)
                {
                    // stretch a bit for  ~ effect ~
                    ScaleSquish.Y += 0.4f;

                    // get a semi-random float that is equivalent to +- X (normalised) degrees around the current rotation
                    float semiRandomAngle = Main.rand.NextFloat(-0.7f, 0.7f);
                    // multiply velocity by a random amount to break up cycles and appear more natural
                    NPC.velocity *= Main.rand.NextFloat(12f, 15f);
                    // check if the velocity lands itself into a tile
                    bool bonk = CalamityUtils.DistanceToTileCollisionHit(NPC.position, NPC.velocity, 9) != null;

                    // random chance if the last direction flip was 3+ hops ago OR if the ghost bell is about to bonk a tile
                    if ((flipCounter >= 3 && Main.rand.NextBool(6)) || bonk)
                    {
                        // send it in the opposite direction
                        NPC.velocity *= -1;
                        NPC.velocity = NPC.velocity.RotatedBy(semiRandomAngle);
                        flipCounter = 0; // reset the counter
                        //Main.NewText("flip! " + flipCounter + " " + NPC.velocity + " " + NPC.rotation);
                    }
                    else
                    {
                        // send it in a random direction
                        NPC.velocity = NPC.velocity.RotatedBy(semiRandomAngle);
                        ++flipCounter; // add to the counter
                        //Main.NewText("swim, " + flipCounter + " " + NPC.velocity + " " + NPC.rotation);
                    }
                }
                NPC.velocity *= 0.95f;
                NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.5f);
                
                if (NPC.velocity == Vector2.Zero)
                {
                    // failsafe. turns out they love getting themselves stuck in walls, so this is necessary to prevent that
                    NPC.velocity = Main.rand.NextVector2CircularEdge(8, 8);
                }
            }
            else
            {
                // wouldnt want it flipping back into the air immediately, would we?
                flipCounter = 0;
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
                case (int)JellyColor.Pink:
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
            }
        }

        public override void ModifyTypeName(ref string typeName)
        {
            if (Variant == (int)JellyColor.Radiant)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.RadiantBabyGhostBell");
            }
            if (Variant == (int)JellyColor.Gold)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.GoldBabyGhostBell");
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
            if (spawnInfo.Player.Calamity().ZoneGleamingBurrows && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
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
                case (int)JellyColor.Pink:
                    dustType = DustID.PinkTorch;
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
                case (int)JellyColor.Pink:
                    texture = PinkTexture.Value;
                    break;
                case (int)JellyColor.Green:
                    texture = GreenTexture.Value;
                    break;
                case (int)JellyColor.Radiant:
                    texture = RadiantTexture.Value;
                    break;
                case (int)JellyColor.Voltaic:
                    texture = VoltaicTexture.Value;
                    break;
                case (int)JellyColor.Gold:
                    texture = GoldTexture.Value;
                    break;
            }
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / Main.npcFrameCount[Type] / 2);
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, npcOffset, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, ScaleSquish, spriteEffects, 0f);

            return false;
        }
    }
}
