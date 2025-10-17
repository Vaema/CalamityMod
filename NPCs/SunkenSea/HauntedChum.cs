using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.DataStructures;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SunkenSea
{
    public class HauntedChum : ModNPC
    {
        public static Asset<Texture2D> jawTexture;
        public List<VerletSimulatedSegment> Segments;

        public override void Load()
        {
            jawTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/HauntedChumMouth");
        }

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 50;
            NPC.defense = 5;
            NPC.lifeMax = 60;
            NPC.damage = 20;
            NPC.aiStyle = -1;
            NPC.alpha = 255;
            AIType = -1;
            NPC.HitSound = SoundID.DD2_SkeletonHurt;
            NPC.DeathSound = SoundID.DD2_SkeletonDeath;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<HauntedChumBanner>();
        }

        public override void AI()
        {
            int aggroRange = 600;
            if (NPC.direction == 0)
            {
                NPC.TargetClosest();
            }
            Player target = Main.player[NPC.target];
            NPC body = Main.npc[(int)NPC.ai[3]];
            // Create a chain
            if (Segments == null || Segments.Count < 10)
            {
                Segments = new List<VerletSimulatedSegment>(10);
                for (int i = 0; i < 10; i++)
                {
                    VerletSimulatedSegment segment = new VerletSimulatedSegment(body.Center + Vector2.UnitY * i * 10);
                    Segments.Add(segment);
                }

                Segments[0].locked = true;
                Segments[Segments.Count - 1].locked = true;
            }
            if (body == null || !body.active || body.type != ModContent.NPCType<FesteringRemains>())
            {
                NPC.StrikeInstantKill();
            }
            switch (NPC.ai[0])
            {
                case 0:
                    NPC.velocity.Y = -1;
                    NPC.ai[1]++;
                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 25;
                    }
                    if (NPC.ai[1] > 30)
                    {
                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                    }
                    break;
                case 1:
                    if (target != null && target.active && target.Distance(body.Center) < aggroRange)
                    {
                        NPC.ai[0] = 2;
                        NPC.ai[1] = 0;
                        NPC.TargetClosest();
                        SoundEngine.PlaySound(SoundID.Zombie7, NPC.Center);
                    }
                    NPC.TargetClosest();
                    NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], -MathHelper.PiOver4, 0.05f);
                    if (NPC.velocity.Length() < 0.25f)
                    {
                        if (NPC.Distance(body.Center) > 320)
                        {
                            Vector2 direction = NPC.DirectionTo(body.Center);
                            direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 4;
                        }
                        else
                        {
                            Vector2 direction = Main.rand.NextVector2Circular(30, 30);
                            direction = direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 4;
                        }
                    }
                    // Lose brightness
                    if (NPC.ai[1] < 30)
                    {
                        NPC.localAI[1] = MathHelper.Max(NPC.localAI[1] - 0.01f, 0);
                    }
                    NPC.velocity *= 0.98f;
                    NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    break;
                case 2:
                    if (target == null || !target.active || target.Distance(body.Center) >= aggroRange)
                    {
                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                    }
                    else
                    {
                        NPC.ai[1]++;
                        float dist = target.Distance(NPC.Center);
                        if (dist < 32 || NPC.ai[1] > 50)
                        {
                            NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], -MathHelper.PiOver4, 0.3f);
                        }
                        else
                        {
                            NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], 0, 0.1f);
                        }
                        // Lose brightness
                        if (NPC.ai[1] < 30)
                        {
                            NPC.localAI[1] = MathHelper.Max(NPC.localAI[1] - 0.02f, 0);
                        }
                        // Gain brightness
                        else if (NPC.ai[1] > 60)
                        {
                            NPC.localAI[1] = MathHelper.Min(NPC.localAI[1] + 0.06f, 0.6f);
                        }
                        if (NPC.ai[1] > 70)
                        {
                            NPC.TargetClosest();
                            Vector2 direction = NPC.DirectionTo(target.Center);
                            direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 7;
                            SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot with { Pitch = 0.8f, Volume = 0.8f }, NPC.Center);
                            NPC.ai[1] = 0;
                        }
                        else
                        {
                            NPC.velocity.X *= 0.99f;
                            NPC.velocity.Y *= 0.999f;
                        }
                    }
                    NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    break;
            }
            NPC.spriteDirection = NPC.direction;
            if (NPC.localAI[1] > 0)
            {
                Lighting.AddLight(NPC.Center, 1 * NPC.localAI[1], 0, 0);
            }
            for (int i = 0; i < 2; i++)
            {
                int dustPos = NPC.spriteDirection == 1 ? -10 : 0;
                if (ChildSafety.Disabled)
                {
                Dust.NewDust(new Vector2(NPC.Center.X + dustPos, NPC.position.Y + NPC.height / 4), 0, NPC.height / 2, DustID.Blood, -NPC.spriteDirection * Main.rand.NextFloat(4f, 5f), Main.rand.NextFloat(-3, 3), Scale: Main.rand.NextFloat(0.6f, 1f));
                }
            }

            // Update the chain
            Segments[0].oldPosition = Segments[0].position;
            Segments[0].position = body.Center + new Vector2(body.spriteDirection * 20, 8);

            Segments[Segments.Count - 1].oldPosition = Segments[Segments.Count - 1].position;
            Segments[Segments.Count - 1].position = body.active ? NPC.Center : body.Center;

            Segments = VerletSimulatedSegment.SimpleSimulation(Segments, 10, loops: 1, gravity: 0.3f);

            NPC.ForceNetUpdate();
        }

        internal float WidthFunction(float completionRatio)
        {
            return MathHelper.Clamp(((float)Math.Cos(completionRatio * 60)) + 2, 1, 2) / 2;
        }

        internal Color ColorFunction(float completionRatio)
        {
            return (int)(completionRatio * 100) % 2 == 0 ? new Color(69, 52, 0) : new Color();
        }

        internal float BackgroundWidthFunction(float completionRatio) => WidthFunction(completionRatio) * 3f;

        internal Color BackgroundColorFunction(float completionRatio)
        {
            return Color.Black;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sandnado, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                // Drop a bone gore for each bone
                if (Segments != null && Segments.Count > 0)
                {
                    for (int i = 0; i < Segments.Count; i++)
                    {
                        VerletSimulatedSegment v = Segments[i];
                        int goreType = i < Segments.Count / 2 ? Mod.Find<ModGore>("ChumBone2").Type : Mod.Find<ModGore>("ChumBone1").Type;
                        if (!Main.dedServ)
                        {
                            Gore.NewGorePerfect(NPC.GetSource_Death(), v.position, new Vector2(Main.rand.NextFloat(-2, 2), -6), goreType);
                        }
                    }
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("HauntedChum").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("HauntedChumMouth").Type, 1f);
                }
            }
        }

        public override bool CheckActive()
        {
            return !(Main.npc[(int)NPC.ai[3]].active && Main.npc[(int)NPC.ai[3]].type == ModContent.NPCType<FesteringRemains>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            List<Vector2> points = new List<Vector2>();
            if (Segments != null && Segments.Count > 0)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    points.Add(Segments[i].position);
                }
            }

            PrimitiveRenderer.RenderTrail(points, new(BackgroundWidthFunction, BackgroundColorFunction), 75);
            PrimitiveRenderer.RenderTrail(points, new(WidthFunction, ColorFunction), 75);

            NPC body = Main.npc[(int)NPC.ai[3]];
            // Draw a chain of bones
            if (body != null && body.active && body.type == ModContent.NPCType<FesteringRemains>())
            {
                if (Segments == null || Segments.Count <= 0)
                {
                    return true;
                }
                for (int i = 0; i < Segments.Count - 1; i++)
                {
                    VerletSimulatedSegment seg = Segments[i];
                    float dist = i > 0 ? Vector2.Distance(seg.position, Segments[i - 1].position) : 0;
                    if (dist <= 2)
                        dist = 2;
                    dist += 2;
                    if (i == Segments.Count - 1)
                    {
                        dist = Vector2.Distance(seg.position, body.Center) + 2;
                    }
                    float rot = 0f;
                    if (i > 0)
                        rot = seg.position.DirectionTo(Segments[i - 1].position).ToRotation();
                    SpriteEffects dir = NPC.Center.X > body.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    Texture2D bone = i < Segments.Count / 2 ? ModContent.Request<Texture2D>(ChumBone.Texture2).Value : ModContent.Request<Texture2D>("CalamityMod/Particles/ChumBone1").Value;
                    spriteBatch.Draw(bone, seg.position - Main.screenPosition, null, Lighting.GetColor((int)(seg.position.X / 16), (int)(seg.position.Y / 16)) * NPC.Opacity, rot, bone.Size() / 2, 1f, dir, 0);
                }
            }

            // Spooky glowey aura effect
            if (NPC.ai[0] == 2 && NPC.ai[1] < 50)
            {
                spriteBatch.EnterShaderRegion(BlendState.Additive);
                Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
                Color glowColor = Color.Red;
                int xOffset = NPC.spriteDirection == 1 ? 16 : 22;
                spriteBatch.Draw(bloom, NPC.position - Main.screenPosition + new Vector2(xOffset, 28), null, glowColor * NPC.localAI[1], 0f, bloom.Size() / 2f, 0.8f, SpriteEffects.None, 0);
                spriteBatch.ExitShaderRegion();
            }

            // Draw the chum itself and its jaw, rotated by localai[0]
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 jawOrigin = new Vector2(NPC.spriteDirection == 1 ? jawTexture.Value.Width - 22 : 22, 4);
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2(texture.Width, texture.Height) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(jawTexture.Value, npcOffset + new Vector2(16 * -NPC.spriteDirection, 4), null, NPC.GetAlpha(drawColor), NPC.localAI[0] * NPC.spriteDirection, jawOrigin, NPC.scale, spriteEffects, 0f);
            spriteBatch.Draw(texture, npcOffset, null, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }
}
