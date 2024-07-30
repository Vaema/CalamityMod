using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PolypPanasea : ModNPC
    {
        #region Textures
        // Welcome to the fish texture wall, have a nice stay, or just collapse this region, either works
        public static Texture2D RadiantTexture;
        public static Texture2D GreenTexture;
        public static Texture2D PurpleTexture;
        public static Texture2D TurquoiseTexture;
        public static Texture2D GoldTexture;
        public static Texture2D TextureCoated;
        public static Texture2D RadiantTextureCoated;
        public static Texture2D GreenTextureCoated;
        public static Texture2D PurpleTextureCoated;
        public static Texture2D TurquoiseTextureCoated;
        public static Texture2D GoldTextureCoated;
        #endregion
        public ref float Variant => ref NPC.ai[1];
        public enum FishColor
        {
            Red = 0,
            Turquoise = 1,
            Green = 2,
            Purple = 3,
            Gold = 4,
            Radiant = 5
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            if (!Main.dedServ)
            {
                RadiantTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRadiant", AssetRequestMode.ImmediateLoad).Value;
                GoldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGold", AssetRequestMode.ImmediateLoad).Value;
                GreenTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGreen", AssetRequestMode.ImmediateLoad).Value;
                PurpleTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaPurple", AssetRequestMode.ImmediateLoad).Value;
                TurquoiseTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaTurquoise", AssetRequestMode.ImmediateLoad).Value;
                TextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRedCoated", AssetRequestMode.ImmediateLoad).Value;
                RadiantTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRadiantCoated", AssetRequestMode.ImmediateLoad).Value;
                GoldTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGoldCoated", AssetRequestMode.ImmediateLoad).Value;
                GreenTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGreenCoated", AssetRequestMode.ImmediateLoad).Value;
                PurpleTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaPurpleCoated", AssetRequestMode.ImmediateLoad).Value;
                TurquoiseTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaTurquoiseCoated", AssetRequestMode.ImmediateLoad).Value;
            }
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
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
            NPC.catchItem = (short)ModContent.ItemType<PolypPanaseaItem>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Panaseas released by the player do not randomize when spawned
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
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
                NPC.ai[2] = 61;
            }
            switch (NPC.ai[1])
            {
                case (int)FishColor.Purple:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case (int)FishColor.Green:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaGreenItem>();
                    break;
                case (int)FishColor.Turquoise:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaTurquoiseItem>();
                    break;
                case (int)FishColor.Gold:
                    {
                        NPC.catchItem = ModContent.ItemType<PolypPanaseaGoldItem>();
                        NPC.rarity = 3;
                    }
                    break;
                case (int)FishColor.Radiant:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaRadiantItem>();
                    break;
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
            CalamityRegularEnemyAI.PassiveSwimmingAI(NPC, Mod, 3, 60f, 0.2f, 0.1f, 4f, 4f, 0.05f);
            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
            NPC.spriteDirection = (NPC.direction > 0) ? 1 : -1;
            NPC.noGravity = true;
            // Assure it cant be caught when collecting panacea
            if (NPC.ai[2] <= 60 && NPC.ai[2] > 0)
            {
                NPC.ai[2]--;
            }
            if (Variant == (int)FishColor.Gold)
            {
                NPC.position += NPC.netOffset;
                Color color = Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
                if (color.R > 20 || color.B > 20 || color.G > 20)
                {
                    int colorVal = color.R;
                    if (color.G > colorVal)
                    {
                        colorVal = color.G;
                    }
                    if (color.B > colorVal)
                    {
                        colorVal = color.B;
                    }
                    colorVal /= 30;
                    if (Main.rand.Next(300) < colorVal)
                    {
                        int golddust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 43, 0f, 0f, 254, new Color(255, 255, 0), 0.5f);
                        Main.dust[golddust].velocity *= 0f;
                    }
                }
                NPC.position -= NPC.netOffset;
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
                case (int)FishColor.Radiant:
                    texture = NPC.ai[2] >= 61 ? RadiantTextureCoated : RadiantTexture;
                    break;
                case (int)FishColor.Gold:
                    texture = NPC.ai[2] >= 61 ? GoldTextureCoated : GoldTexture;
                    break;
                case (int)FishColor.Purple:
                    texture = NPC.ai[2] >= 61 ? PurpleTextureCoated : PurpleTexture;
                    break;
                case (int)FishColor.Green:
                    texture = NPC.ai[2] >= 61 ? GreenTextureCoated : GreenTexture;
                    break;
                case (int)FishColor.Turquoise:
                    texture = NPC.ai[2] >= 61 ? TurquoiseTextureCoated : TurquoiseTexture;
                    break;
                case (int)FishColor.Red:
                    texture = NPC.ai[2] >= 61 ? TextureCoated : TextureAssets.Npc[NPC.type].Value;
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
                if (NPC.ai[2] > 60)
                {
                    Item.NewItem(NPC.GetSource_CatchEntity(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, 1, 1, ItemID.FlaskofPoison);
                    NPC.ai[2] = 60;
                }
                return false;
            }            
            return null;
        }
    }
}
