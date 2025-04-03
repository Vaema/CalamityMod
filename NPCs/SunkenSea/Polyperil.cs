using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Polyperil : SunkenSeaNPC
    {
        public ref float Color => ref NPC.ai[1];

        public ref float Timer => ref NPC.ai[0];

        public static Asset<Texture2D> blueTexture = null;

        public static Asset<Texture2D> greenTexture = null;

        public static Asset<Texture2D> pinkTexture = null;

        public static Asset<Texture2D> radiantTexture = null;

        // The tentacles do all the work
        protected override List<int> PreyIDs => new List<int>();

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<KelpieSeadragon>()
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs | SunkenSeaBiomeFlags.PolypForest;

        public override void Load()
        {
            pinkTexture = ModContent.Request<Texture2D>(Texture + "Pink");
            blueTexture = ModContent.Request<Texture2D>(Texture + "Blue");
            greenTexture = ModContent.Request<Texture2D>(Texture + "Green");
            radiantTexture = ModContent.Request<Texture2D>(Texture + "Radiant");
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.damage = 20;
            NPC.lifeMax = 200;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.chaseable = false;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.width = 32;
            NPC.height = 36;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            // Banner = NPC.type;
            // BannerItem = ModContent.ItemType<PolyperilBanner>();

            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            // Scale stats in Expert and Master
            CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Pick a random color
            Color = Main.rand.Next(0, 3);
            // 1 in 30 chance to be Radiant
            if (Main.rand.NextBool(30))
                Color = 3;

            // Spawn tentacles
            int dist = 80;
            int tentMin = 3;
            int tentMax = 8;
            for (int i = 0; i < Main.rand.Next(tentMin, tentMax + 1); i++)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 pos = NPC.Center + new Vector2(Main.rand.Next(-dist, dist), Main.rand.Next(-dist, dist));
                    NPC tent = NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<PolyperilTentacle>(), ai0: NPC.whoAmI);
                    tent.ModNPC<PolyperilTentacle>().anchor = pos;
                }
            }
        }

        public override void AI()
        {
            Timer++;
            NPC.velocity.Y += 10;

            // While out of water, slowly lose health (suffocating)
            if (!NPC.wet)
            {
                if (Timer % 60 == 0)
                {
                    NPC.HitEffect(0, 5);
                    NPC.life -= 5;
                    if (NPC.life <= 0)
                        NPC.life = 1;
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            return PredatorIDs.Contains(attacker.type);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZonePolypForest)
                    return SpawnCondition.CaveJellyfish.Chance * 0.6f;
                
                else if (spawnInfo.Player.Calamity().ZoneRadiantReefs)
                    return SpawnCondition.CaveJellyfish.Chance * 0.05f;
            }
            return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = Color switch
            {
                1 => blueTexture.Value,
                2 => greenTexture.Value,
                3 => radiantTexture.Value,
                _ => pinkTexture.Value
            };

            spriteBatch.Draw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, null, NPC.GetAlpha(drawColor), NPC.rotation, tex.Size() / 2, NPC.scale, SpriteEffects.None, 0);

            // Desaturate in color when losing health, based on real anemones
            float vibrance = MathHelper.Lerp(1, 0, Utils.GetLerpValue(1, 0.33f, NPC.life / (float)NPC.lifeMax, true));

            spriteBatch.Draw(tex, NPC.Center - screenPos, null, NPC.GetAlpha(drawColor) * vibrance, NPC.rotation, tex.Size() / 2, NPC.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
