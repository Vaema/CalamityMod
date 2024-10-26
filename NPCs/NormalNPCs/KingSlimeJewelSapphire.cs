using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class KingSlimeJewelSapphire : ModNPC
    {
        private const int BuffDustGateValue = 60;
        private const float LightTelegraphDuration = 45f;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NeedsExpertScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 0;
            NPC.width = 32;
            NPC.height = 32;
            NPC.defense = 5;
            NPC.DR_NERD(0.05f);
            NPC.lifeMax = 120;
            NPC.knockBackResist = 0.9f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath15;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void AI()
        {
            // Despawn
            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                NPC.life = 0;
                OnKill();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            Lighting.AddLight(NPC.Center, 0f, 0f, 0.8f);

            // Float around the player
            NPC.rotation = NPC.velocity.X / 15f;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                CalamityUtils.CalamityTargeting(NPC, default);
            }

            float velocity = 5f;
            float acceleration = 0.1f;

            int distanceFromKingSlime = 1;
            Vector2 kingSlimeCenter = NPC.Center;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPCID.KingSlime)
                {
                    distanceFromKingSlime = (int)NPC.Distance(Main.npc[i].Center);
                    kingSlimeCenter = Main.npc[i].Center;
                    break;
                }
            }

            Vector2 movementTarget = kingSlimeCenter == NPC.Center ? Main.player[NPC.target].Center : kingSlimeCenter;
            if (NPC.position.Y > movementTarget.Y - 200f)
            {
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= 0.98f;

                NPC.velocity.Y -= acceleration;

                if (NPC.velocity.Y > velocity)
                    NPC.velocity.Y = velocity;
            }
            else if (NPC.position.Y < movementTarget.Y - 250f)
            {
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y *= 0.98f;

                NPC.velocity.Y += acceleration;

                if (NPC.velocity.Y < -velocity)
                    NPC.velocity.Y = -velocity;
            }

            if (NPC.Center.X > movementTarget.X + 100f)
            {
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X *= 0.98f;

                NPC.velocity.X -= acceleration;

                if (NPC.velocity.X > 8f)
                    NPC.velocity.X = 8f;
            }
            if (NPC.Center.X < movementTarget.X - 100f)
            {
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X *= 0.98f;

                NPC.velocity.X += acceleration;

                if (NPC.velocity.X < -8f)
                    NPC.velocity.X = -8f;
            }

            // Emit buff dust
            NPC.ai[0] += 1f;
            if (NPC.ai[0] >= BuffDustGateValue)
            {
                NPC.ai[0] = 0f;

                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                for (int dusty = 0; dusty < 10; dusty++)
                {
                    Vector2 dustVel = (kingSlimeCenter - NPC.Center).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2f, 4f);
                    int sapphire = Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                    Main.dust[sapphire].velocity = dustVel * Main.rand.NextFloat(1f, 2f);
                    Main.dust[sapphire].noGravity = true;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[sapphire].scale = 0.5f;
                        Main.dust[sapphire].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                int maxDustIterations = distanceFromKingSlime;
                int maxDust = 100;
                int dustDivisor = maxDustIterations / maxDust;
                if (dustDivisor < 2)
                    dustDivisor = 2;

                Vector2 dustLineStart = NPC.Center;
                Vector2 dustLineEnd = kingSlimeCenter;
                Vector2 currentDustPos = default;
                Vector2 spinningpoint = new Vector2(0f, -1f).RotatedByRandom(MathHelper.Pi);
                int dustSpawned = 0;
                for (int i = 0; i < maxDustIterations; i++)
                {
                    if (i % dustDivisor == 0)
                    {
                        currentDustPos = Vector2.Lerp(dustLineStart, dustLineEnd, i / (float)maxDustIterations);
                        int dust = Dust.NewDust(currentDustPos, 0, 0, DustID.GemSapphire, 0f, 0f, 100, default, 1f);
                        Main.dust[dust].position = currentDustPos;
                        Main.dust[dust].velocity = spinningpoint.RotatedBy(MathHelper.TwoPi * i / maxDustIterations) * (0.9f + Main.rand.NextFloat() * 0.2f);
                        Main.dust[dust].noGravity = true;
                        if (Main.rand.NextBool())
                        {
                            Main.dust[dust].scale = 0.5f;
                            Main.dust[dust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                        }

                        Dust dust2 = Dust.CloneDust(dust);
                        Dust dust3 = dust2;
                        dust3.scale *= 0.5f;
                        dust3 = dust2;
                        dust3.fadeIn *= 0.5f;
                        dustSpawned++;
                    }
                }

                NPC.netUpdate = true;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Color col = Color.BlueViolet;
            Color flashCol = Color.LightSkyBlue;

            NPC.localAI[1]++;

            float alph = 0.4f + (float)(Math.Sin(NPC.localAI[1] / 30f) * 0.4f);

            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/NPCs/NormalNPCs/KingSlimeJewelFlash");

            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos, tex.Frame(), Color.White, NPC.rotation, tex.Frame().Center(), 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(tex2.Value, NPC.Center - screenPos, tex2.Frame(), Color.Lerp(col, flashCol, alph).MultiplyRGBA(new Color(alph, alph, alph, 0f)), NPC.rotation, tex2.Frame().Center(), alph * 1.2f, SpriteEffects.None);

            return false;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            Color initialColor = new Color(100, 100, 175);
            Color newColor = initialColor;
            Color finalColor = new Color(150, 150, 255);
            float colorTelegraphGateValue = BuffDustGateValue - LightTelegraphDuration;
            if (NPC.ai[0] > colorTelegraphGateValue)
                newColor = Color.Lerp(initialColor, finalColor, (NPC.ai[0] - colorTelegraphGateValue) / LightTelegraphDuration);
            newColor.A = (byte)(255 * NPC.Opacity);

            return newColor;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override void OnKill()
        {
            for (int i = 0; i < 6; i++)
            {
                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.BlueViolet));
                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.LightSkyBlue));
            }

            float start = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 3; i++)
                GeneralParticleHandler.SpawnParticle(new CustomSprite(NPC.Center, new Vector2(0, -2).RotatedByRandom(start + MathHelper.ToRadians(20f)).RotatedBy(MathHelper.ToRadians(i * 125)), 120, "CalamityMod/Particles/KingSlimeSapphireShards", 1f, new Color(255, 255, 255), Main.rand.NextFloat(0.2f, 0.6f), frameCount: 3, frame: i));

            SoundEngine.PlaySound(KingSlimeJewelRuby.ShatterSound, NPC.Center);
        }

        public override bool CheckActive() => false;

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, hit.HitDirection, -1f, 0, default, 1f);
            Main.dust[dust].noGravity = true;

            if (NPC.life <= 0)
            {
                NPC.position = NPC.Center;
                NPC.width = NPC.height = 45;
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);

                for (int i = 0; i < 2; i++)
                {
                    int rubyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                    Main.dust[rubyDust].noGravity = true;
                    Main.dust[rubyDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[rubyDust].scale = 0.5f;
                        Main.dust[rubyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 10; j++)
                {
                    int rubyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 3f);
                    Main.dust[rubyDust2].noGravity = true;
                    Main.dust[rubyDust2].velocity *= 5f;
                    rubyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                    Main.dust[rubyDust2].noGravity = true;
                    Main.dust[rubyDust2].velocity *= 2f;
                }
            }
        }
    }
}
