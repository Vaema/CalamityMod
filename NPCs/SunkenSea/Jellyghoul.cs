using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.DataStructures;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Systems;
using CalamityMod.Walls;
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
    public class Jellyghoul : ModNPC
    {
        public static Color lesserRed = new Color(255, 100, 100);
        public static Color startColor = new Color(111, 115, 122);
        public static Color endColor = new Color(87, 103, 113);

        // Cycle between two gray colors by default
        public Color DefaultColor => Color.Lerp(startColor, endColor, NPC.localAI[1] * 0.25f + MathF.Sin(Main.GlobalTimeWrappedHourly));

        // Turn red when enraged, otherwise just continue using DefaultColor
        public Color FinalColor => NPC.ai[2] > 0 ? Color.Lerp(DefaultColor, lesserRed, NPC.ai[2]) : DefaultColor;


        public static SoundStyle JellyGhoulScream = SoundID.NPCDeath51 with { Pitch = -1, Volume = 0.2f, MaxInstances = 0 };

        public static Asset<Texture2D> tentacleTexture;

        public override void Load()
        {
            tentacleTexture = ModContent.Request<Texture2D>(Texture + "Tentacle");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 1f;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 28;
            NPC.height = 36;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0f;
            NPC.alpha = 100;
            NPC.HitSound = null; // instantly dies
            NPC.DeathSound = SoundID.NPCDeath3;
            NPC.chaseable = false;
            NPC.noTileCollide = true;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<JellyghoulBanner>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Jellyghoul")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.position -= new Vector2(Main.rand.NextFloat(-16 * 16, 16 * 16), Main.rand.NextFloat(4 * 16, 16 * 16));
        }
        public override void AI()
        {
            // LocalAI controls variance between jellies. Randomize it on spawn to desync them
            if (NPC.localAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.localAI[0] = 1f;
                NPC.localAI[1] = Main.rand.NextFloat(0, 4f);
                NPC.netUpdate = true;
            }

            Lighting.AddLight(NPC.Center, FinalColor.R * 0.002f, FinalColor.G * 0.002f, FinalColor.B * 0.002f);

            NPC.ai[0]++;
            // Play a scream randomly with a 5 second cooldown
            // Chance decreases with amount of jellies active for sanity
            if (NPC.ai[0] > 300 && Main.rand.NextBool(300 * NPC.CountNPCS(Type)) && NPC.ai[1] == 0)
            {
                SoundEngine.PlaySound(JellyGhoulScream, NPC.Center);
                NPC.ai[1] = 1;
            }
            // While screaming, shake the screen
            if (NPC.ai[1] > 0)
            {
                NPC.ai[1]++;

                float screenShakePower = 2 * Utils.GetLerpValue(500f, 0f, NPC.Distance(Main.LocalPlayer.Center), true);
                if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakePower)
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower;

                // After 1 second, stop shaking and set the scream on cooldown
                if (NPC.ai[1] > 60)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 0;
                }
            }
            // Controls enrage coloring
            if (NPC.ai[2] > 0)
            {
                NPC.ai[2] += 0.01f;
                if (NPC.ai[2] > 1f)
                {
                    NPC.ai[2] = 1f;
                }
            }
            if (NPC.ai[2] >= 1)
            {
                NPC.chaseable = true;
                NPC.TargetClosest(false);
                NPC.ai[3]++;
                if (NPC.ai[3] >= 120 + (NPC.localAI[1] * 10))
                {
                    int damage = Main.masterMode ? 4 : Main.expertMode ? 6 : 8;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SupremeCalamitas.SupremeCalamitas.BrimstoneShotSound with { Pitch = 1f });
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.SafeDirectionTo(Main.player[NPC.target].Center) * 16, ModContent.ProjectileType<JellyghoulBolt>(), damage, 0f, Main.myPlayer);
                    }
                    NPC.ai[3] = 0;
                }
            }
            if (Main.rand.NextBool(80))
            { 
                Particle ash = new SquareAshParticle(Main.rand.NextVector2FromRectangle(NPC.getRect()), new Vector2(0, 2), Main.rand.Next(100, 200), Main.rand.NextFloat(0.8f, 1.2f), new Color(50, 50, 50));
                GeneralParticleHandler.SpawnParticle(ash);
            }
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
            if (spawnInfo.Player.Calamity().ZoneTimelessShores && !spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.Cavern.Chance * 0.9f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SpectreStaff, hit.HitDirection, -1f, 0, Color.DarkGray * 0.2f, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SpectreStaff, hit.HitDirection, -1f, 0, Color.DarkGray * 0.2f, 1f);
                }
            }
            CalamityUtils.SpawnGores(NPC, "Jellyghoul", 2);
        }

        public override void OnKill()
        {
            bool soundPlayed = false;
            // Cause nearby ghouls to become angry on death
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.life < 0)
                    continue;
                if (n.type != Type)
                    continue;
                if (n.Distance(NPC.Center) > 1000)
                    continue;
                // If the jelly isn't enraged yet, enrage it
                if (n.ai[2] == 0)
                {
                    n.ai[2] = 0.01f;
                    // Play a sound if at least one jelly successfully enraged
                    if (!soundPlayed)
                    {
                        SoundEngine.PlaySound(SoundID.Zombie83 with { Pitch = -0.5f, Volume = 1.6f }, NPC.Center);
                        soundPlayed = true;
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            float afterimageRotationSpeed = 1;
            float afterimageDistance = 2;
            float rotationMultiplier = 0.15f;
            // Make the jelly visually move in circles 
            Vector2 afterimageOffset = new Vector2((float)Math.Cos(Main.GlobalTimeWrappedHourly * afterimageRotationSpeed) + NPC.localAI[1], (float)Math.Sin(Main.GlobalTimeWrappedHourly * afterimageRotationSpeed) + NPC.localAI[1]) * afterimageDistance;

            // Spooky glowey aura effect
            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/SmallBloom").Value;

                spriteBatch.Draw(bloom, NPC.Center - screenPos + afterimageOffset, null, FinalColor * 0.45f, 0f, bloom.Size() / 2f, 0.6f, SpriteEffects.None, 0);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / Main.npcFrameCount[Type] / 2));

            // Rotate back and forth like a bell
            float rotation = NPC.rotation + MathF.Sin(Main.GlobalTimeWrappedHourly + NPC.localAI[1]) * rotationMultiplier;

            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[Type])) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);

            // Tentacles
            Texture2D tentacle = tentacleTexture.Value;
            for (int i = 0; i < 4; i++)
            {
                // Is the tentacle on the outside?
                bool outerTentacle = i == 0 || i == 3;
                // Is the tentacle one of the left ones?
                int leftTentacle = i < 2 ? -1 : 1;
                // Start position x offset
                int xOffset = outerTentacle ? 4 : 2;
                // How much the end position is rotated by
                int endDeg = outerTentacle ? 20 : 5;
                // Length of each tentacle
                int tentacleLength = outerTentacle ? 60 : 50;
                // A psuedo randomness value between tentacles and the NPC's index used to desync tentacles from each other
                int randomness = i % 2 + NPC.whoAmI;
                // Start position of the tentacle
                Vector2 anchor = npcOffset + afterimageOffset + new Vector2(leftTentacle * xOffset, 10);
                // End position of the tentacle
                Vector2 end = npcOffset + afterimageOffset + Vector2.UnitY.RotatedBy(MathHelper.ToRadians(endDeg * -leftTentacle)).RotatedBy(MathHelper.ToRadians(MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 5 * -leftTentacle)) * tentacleLength;

                // Store the previous tentacle's location
                Vector2 prevTentacle = npcOffset + afterimageOffset;
                int segmentCount = 10;
                Vector2 direction = anchor.DirectionTo(end).RotatedBy(MathHelper.PiOver2);
                BezierCurve curve = new BezierCurve(anchor, anchor + (end - anchor) * 0.33f + direction * MathF.Sin(Main.GlobalTimeWrappedHourly * 4 + randomness) * 8, anchor + (end - anchor) * 0.66f + direction * MathF.Cos(Main.GlobalTimeWrappedHourly * 4 + randomness) * 16, end);
                List<Vector2> points = curve.GetPoints(segmentCount);
                for (int j = 0; j < segmentCount; j++)
                {
                    // Which texture variant the tentacle uses is based on the horizontal position on its sheet
                    int texX = i == 0 ? 0 : i == (segmentCount - 1) ? 16 : 8;
                    // Which segment type.
                    // The first segment (0 is invisible) uses the first frame
                    // The final segment uses the last frame
                    // The rest alternate between frames 2 and 3
                    int texY = j % 2 == 0 ? 16 : 8;
                    if (j == 1)
                        texY = 0;
                    else if (j == segmentCount - 1)
                        texY = 24;

                    int segHeight = (j == segmentCount - 1) ? 8 : 6;

                    if (j != 0)
                        spriteBatch.Draw(tentacle, points[j], new Rectangle(texX, texY, 6, segHeight), FinalColor * NPC.Opacity, points[j].DirectionTo(prevTentacle).ToRotation() + MathHelper.PiOver2, new Vector2(2), NPC.scale, spriteEffects, 0);
                    prevTentacle = points[j];
                }
            }

            // Draws transparent clones around itself to look woozy
            int cloneAmt = 4;
            for (int i = 0; i < cloneAmt; i++)
            {
                spriteBatch.Draw(texture, npcOffset + afterimageOffset + Vector2.One.RotatedBy((i + 1) * (MathHelper.TwoPi / cloneAmt) + Main.GlobalTimeWrappedHourly + NPC.localAI[1]) * cloneAmt * MathF.Sin(Main.GlobalTimeWrappedHourly + NPC.localAI[1]), NPC.frame, FinalColor * NPC.Opacity * 0.2f, rotation, origin, NPC.scale, spriteEffects, 0f);
            }
            spriteBatch.Draw(texture, npcOffset + afterimageOffset, NPC.frame, FinalColor * NPC.Opacity, rotation, origin, NPC.scale, spriteEffects, 0f);

            if (!NPC.HasValidTarget)
                return false;

            // Telegraph
            float shotGate = 30 + (NPC.localAI[1] * 10);
            float shotCompletion = (NPC.ai[3] - 90) / shotGate;

            Effect effect = Terraria.Graphics.Effects.Filters.Scene["CalamityMod:SpreadTelegraph"].GetShader().Shader;
            effect.Parameters["centerOpacity"].SetValue(MathHelper.Lerp(0, 0.7f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.05f, shotCompletion));
            effect.Parameters["mainOpacity"].SetValue(MathHelper.Lerp(0, 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.05f, shotCompletion));
            effect.Parameters["halfSpreadAngle"].SetValue(MathHelper.Lerp(0, MathHelper.PiOver4 * 0.33f, shotCompletion));
            effect.Parameters["edgeColor"].SetValue(Color.DarkRed.ToVector3());
            effect.Parameters["centerColor"].SetValue(Color.Red.ToVector3());
            effect.Parameters["edgeBlendLength"].SetValue(0.07f);
            effect.Parameters["edgeBlendStrength"].SetValue(4f);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive, effect);

            Texture2D invis = ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

            Main.EntitySpriteDraw(invis, npcOffset + afterimageOffset, null, Color.White, NPC.DirectionTo(Main.player[NPC.target].Center).ToRotation(), new Vector2(invis.Width / 2f, invis.Height / 2f), 500f, 0, 0);

            Main.spriteBatch.ExitShaderRegion();

            return false;
        }
    }
}
