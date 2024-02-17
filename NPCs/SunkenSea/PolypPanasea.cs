using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PolypPanasea : ModNPC
    {
        #region Textures
        // Welcome to the fish texture wall, have a nice stay, or just collapse this region, either works
        public static Texture2D DeluxTexture;
        public static Texture2D GreenTexture;
        public static Texture2D PurpleTexture;
        public static Texture2D TurquoiseTexture;
        public static Texture2D RadiantTexture;
        public static Texture2D TextureCoated;
        public static Texture2D DeluxTextureCoated;
        public static Texture2D GreenTextureCoated;
        public static Texture2D PurpleTextureCoated;
        public static Texture2D TurquoiseTextureCoated;
        public static Texture2D RadiantTextureCoated;
        #endregion
        public ref float Variant => ref NPC.ai[1];
        public enum FishColor
        {
            Red = 0,
            Turquoise = 1,
            Green = 2,
            Purple = 3,
            Radiant = 4,
            Blue = 5
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            if (!Main.dedServ)
            {
                DeluxTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaDelux", AssetRequestMode.ImmediateLoad).Value;
                RadiantTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRadiant", AssetRequestMode.ImmediateLoad).Value;
                GreenTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGreen", AssetRequestMode.ImmediateLoad).Value;
                PurpleTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaPurple", AssetRequestMode.ImmediateLoad).Value;
                TurquoiseTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaTurquoise", AssetRequestMode.ImmediateLoad).Value;
                TextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaCoated", AssetRequestMode.ImmediateLoad).Value;
                DeluxTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaDeluxCoated", AssetRequestMode.ImmediateLoad).Value;
                RadiantTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRadiantCoated", AssetRequestMode.ImmediateLoad).Value;
                GreenTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGreenCoated", AssetRequestMode.ImmediateLoad).Value;
                PurpleTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaPurpleCoated", AssetRequestMode.ImmediateLoad).Value;
                TurquoiseTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaTurquoiseCoated", AssetRequestMode.ImmediateLoad).Value;
            }
            //Main.npcCatchable[NPC.type] = true;
            //NPCID.Sets.CountsAsCritter[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 36;
            NPC.height = 22;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<PolypPanaseaBanner>();
            NPC.chaseable = false;
            //NPC.catchItem = (short)ModContent.ItemType<SeaMinnowItem>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Randomize the color of the fish
            NPC.ai[1] = Main.rand.Next(0, 4);
            // 1 in 30 chance for a rare fish variant (rfv)
            if (Main.rand.NextBool(30))
            {
                NPC.ai[1] = Main.rand.Next(4, 6);
            }
            // 1 in 5 chance for a Panasea to be coated
            if (Main.rand.NextBool(5))
            {
                NPC.ai[2] = 121;
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PolypPanasea")
            });
        }

        public override void AI()
        {
            NPC.catchItem = NPC.ai[2] > 120 ? ItemID.FlaskofPoison : ModContent.ItemType<SeaMinnowItem>();
            CalamityAI.PassiveSwimmingAI(NPC, Mod, 3, 60f, 0.2f, 0.1f, 4f, 4f, 0.05f);
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.spriteDirection = (NPC.direction > 0) ? 1 : -1;
            NPC.noGravity = true;
            if (NPC.ai[2] <= 120 && NPC.ai[2] > 0)
            {
                NPC.ai[2]--;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter += 0.075f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
            NPC.ai[3] = frame;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSeaPolyp && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
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
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            switch (Variant)
            {
                case (int)FishColor.Blue:
                    texture = NPC.ai[2] >= 121 ? DeluxTextureCoated : DeluxTexture;
                    break;
                case (int)FishColor.Radiant:
                    texture = NPC.ai[2] >= 121 ? RadiantTextureCoated : RadiantTexture;
                    break;
                case (int)FishColor.Purple:
                    texture = NPC.ai[2] >= 121 ? PurpleTextureCoated : PurpleTexture;
                    break;
                case (int)FishColor.Green:
                    texture = NPC.ai[2] >= 121 ? GreenTextureCoated : GreenTexture;
                    break;
                case (int)FishColor.Turquoise:
                    texture = NPC.ai[2] >= 121 ? TurquoiseTextureCoated : TurquoiseTexture;
                    break;
                case (int)FishColor.Red:
                    texture = NPC.ai[2] >= 121 ? TextureCoated : TextureAssets.Npc[NPC.type].Value;
                    break;
            }
            Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / Main.npcFrameCount[NPC.type] / 2));
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[NPC.type])) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            Rectangle frame = texture.Frame(1, 6, 0, (int)NPC.ai[3]);
            spriteBatch.Draw(texture, npcOffset, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
        public override bool? CanBeCaughtBy(Item item, Player player)
        {
            if (NPC.ai[2] > 0)
            {
                if (NPC.ai[2] > 120)
                {
                    Item.NewItem(NPC.GetSource_CatchEntity(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, 1, 1, NPC.catchItem);
                    NPC.ai[2] = 120;
                }
                return false;
            }            
            return null;
        }
    }
}
