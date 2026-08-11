using System;
using CalamityMod.Events;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using CalamityMod.Systems.Collections;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs;

public sealed partial class CalamityVanillaAIOverrideNPC : GlobalNPC
{
    private static bool GlobalPreAI(NPC npc)
    {
        var calNPC = npc.GetGlobalNPC<CalamityGlobalNPC>();

        // Don't do damage for 42 frames after spawning in
        if (npc.type == NPCID.Sharkron || npc.type == NPCID.Sharkron2)
        {
            npc.damage = npc.alpha > 0 ? 0 : npc.defDamage;
        }

        // Servant of Cthulhu light
        else if (npc.type == NPCID.ServantofCthulhu)
        {
            Lighting.AddLight(npc.Center, 0.2f, 0.2f, 0.2f);
        }

        // Cultist Boss light and hitbox changes
        else if (npc.type == NPCID.CultistBoss)
        {
            // Emit light
            float lifeRatio = npc.life / (float)npc.lifeMax;
            float colorTransitionAmt = (float)Math.Pow((double)(1f - lifeRatio), 2D);
            Color lightColor = Color.Lerp(Color.Cyan, Color.Blue, colorTransitionAmt);
            Lighting.AddLight(npc.Center, lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f);

            // Decrement the hit counter for the shield flicker
            if (calNPC.newAI[1] > 0f)
                calNPC.newAI[1] -= 1f;

            // Cultist shield hitbox
            Vector2 hitboxSize = new Vector2(216f / 1.4142f);
            if (npc.Size != hitboxSize)
                npc.Size = hitboxSize;
        }

        // Cultist Clone Light
        else if (npc.type == NPCID.CultistBossClone)
        {
            if (Main.npc[(int)npc.ai[3]].active)
            {
                // Emit light
                float lifeRatio = Main.npc[(int)npc.ai[3]].life / (float)Main.npc[(int)npc.ai[3]].lifeMax;
                float colorTransitionAmt = (float)Math.Pow((double)(1f - lifeRatio), 2D);
                Color lightColor = Color.Lerp(Color.Cyan, Color.Blue, colorTransitionAmt);
                Lighting.AddLight(npc.Center, lightColor.R / 255f, lightColor.G / 255f, lightColor.B / 255f);
            }
        }

        return true;
    }

    private static void GlobalAI(NPC npc)
    {
        // Fair contact damage
        switch (npc.type)
        {
            case NPCID.DD2Betsy:
                npc.damage = npc.ai[0] == 2f ? npc.defDamage : 0;
                break;

            case NPCID.DD2WyvernT1:
            case NPCID.DD2WyvernT2:
            case NPCID.DD2WyvernT3:
                npc.damage = npc.ai[0] == 2f ? npc.defDamage : 0;
                break;

            case NPCID.Mothron:
                npc.damage = npc.ai[0] == 3.2f ? (int)Math.Round(npc.defDamage * 1.3) : npc.ai[0] == 2f ? (int)Math.Round(npc.defDamage * 0.5) : 0;
                break;

            case NPCID.MothronSpawn:
                npc.damage = npc.ai[0] == 2.1f ? npc.defDamage : 0;
                break;

            case NPCID.Mimic:
            case NPCID.IceMimic:
            case NPCID.PresentMimic:
                npc.damage = (npc.ai[0] == 0f || npc.velocity.Y == 0f) ? 0 : npc.defDamage;
                break;

            case NPCID.BigMimicCorruption:
            case NPCID.BigMimicCrimson:
            case NPCID.BigMimicHallow:
            case NPCID.BigMimicJungle:
                npc.damage = npc.ai[0] == 3f ? 0 : npc.defDamage;

                // Spend less time in closed state
                if (npc.ai[0] == 3f)
                    npc.ai[1] += 0.5f;

                break;

            case NPCID.MartianDrone:
            case NPCID.SolarCorite:
                npc.damage = (npc.ai[0] == 2f || npc.ai[0] == 3f) ? npc.defDamage : 0;
                break;

            case NPCID.GraniteFlyer:
                npc.damage = npc.ai[0] == -1f ? 0 : npc.defDamage;
                break;

            case NPCID.GraniteGolem:
                npc.damage = npc.ai[2] < 0f ? 0 : npc.defDamage;
                break;

            case NPCID.BlueSlime:
            case NPCID.MotherSlime:
            case NPCID.LavaSlime:
            case NPCID.DungeonSlime:
            case NPCID.CorruptSlime:
            case NPCID.IlluminantSlime:
            case NPCID.ToxicSludge:
            case NPCID.IceSlime:
            case NPCID.Crimslime:
            case NPCID.UmbrellaSlime:
            case NPCID.RainbowSlime:
            case NPCID.SlimeMasked:
            case NPCID.HoppinJack:
            case NPCID.SlimeRibbonWhite:
            case NPCID.SlimeRibbonYellow:
            case NPCID.SlimeRibbonGreen:
            case NPCID.SlimeRibbonRed:
            case NPCID.SandSlime:
            case NPCID.GoldenSlime:
            case NPCID.ShimmerSlime:
            case NPCID.GreenSlime:
            case NPCID.RedSlime:
            case NPCID.PurpleSlime:
            case NPCID.YellowSlime:
            case NPCID.BlackSlime:
            case NPCID.JungleSlime:
            case NPCID.BabySlime:
            case NPCID.Pinky:
            case NPCID.Slimeling:
            case NPCID.Slimer2:
                npc.damage = (npc.velocity.Y == 0f || npc.velocity.Length() < 3f) ? 0 : npc.defDamage;
                break;

            case NPCID.GiantShelly:
            case NPCID.GiantShelly2:
                npc.damage = npc.ai[0] == 3f ? (int)Math.Round(npc.defDamage * 1.2) : 0;
                break;

            case NPCID.GiantTortoise:
            case NPCID.IceTortoise:
                npc.damage = npc.ai[0] == 3f ? (int)Math.Round(npc.defDamage * 1.4) : 0;
                break;

            case NPCID.SolarSroller:
                npc.damage = npc.ai[0] == 6f ? (int)Math.Round(npc.defDamage * 1.2) : 0;
                break;

            default:
                break;
        }
    }

    private static bool GlobalPreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var calNPC = npc.GetGlobalNPC<CalamityGlobalNPC>();
        bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
        bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

        // Destroyer drawing and laser telegraphs
        if (CalamityNPCTypeSets.Destroyer.Contains(npc.type) && !npc.IsABestiaryIconDummy)
        {
            Texture2D npcTexture = TextureAssets.Npc[npc.type].Value;
            Vector2 halfSize = npc.frame.Size() / 2;
            SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color segmentDrawColor = npc.GetAlpha(drawColor);

            // Check if Destroyer is behind tiles and, if so, how much of the segment is behind tiles and adjust color accordingly
            int x = (int)((npc.position.X - 8f) / 16f);
            int x2 = (int)((npc.position.X + npc.width + 8f) / 16f);
            int y = (int)((npc.position.Y - 8f) / 16f);
            int y2 = (int)((npc.position.Y + npc.height + 8f) / 16f);
            for (int l = x; l <= x2; l++)
            {
                for (int m = y; m <= y2; m++)
                {
                    if (Lighting.Brightness(l, m) == 0f)
                        segmentDrawColor = Color.Black;
                }
            }

            // Draw segments
            spriteBatch.Draw(npcTexture, npc.Center - screenPos + new Vector2(0, npc.gfxOffY), npc.frame, segmentDrawColor, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);

            // Draw lights
            if (npc.ai[2] == 0f && segmentDrawColor != Color.Black)
            {
                // This life ratio is fine now because all Destroyer segments update to have the same amount of life every frame
                float destroyerLifeRatio = npc.life / (float)npc.lifeMax;

                // Phases
                bool phase4 = destroyerLifeRatio < (death ? 0.4f : 0.25f);
                bool phase5 = destroyerLifeRatio < (death ? 0.2f : 0.1f);

                // Spawn DR check
                bool hasSpawnDR = calNPC.newAI[1] < DestroyerAI.DRIncreaseTime && calNPC.newAI[1] > 60f;

                // Gradual color transition from ground to flight and vice versa
                // 0f = Red, 1f = Purple
                float phaseTransitionColorAmount = (hasSpawnDR || phase5) ? 1f : 0f;
                if (!hasSpawnDR && !phase5)
                {
                    if (calNPC.newAI[3] >= DestroyerAI.GroundTelegraphStartGateValue)
                        phaseTransitionColorAmount = MathHelper.Clamp(1f - (calNPC.newAI[3] - DestroyerAI.GroundTelegraphStartGateValue) / DestroyerAI.PhaseTransitionTelegraphTime, 0f, 1f);
                    else if (calNPC.newAI[3] >= DestroyerAI.FlightTelegraphStartGateValue)
                        phaseTransitionColorAmount = MathHelper.Clamp((calNPC.newAI[3] - DestroyerAI.FlightTelegraphStartGateValue) / DestroyerAI.PhaseTransitionTelegraphTime, 0f, 1f);
                }

                // Light colors
                int alpha = 192;
                Color groundColor = new Color(255, 125, 125, alpha);
                Color flightColor = revenge ? new Color(125, 0, 255, alpha) : groundColor;
                Color segmentColor = Color.Lerp(groundColor, flightColor, phaseTransitionColorAmount);
                Color telegraphColor_Red = new Color(255, 125, 125, alpha);
                Color telegraphColor_Green = new Color(125, 255, 125, alpha);
                Color telegraphColor_Cyan = new Color(0, 255, 255, alpha);
                Color telegraphColor = telegraphColor_Red;

                // Telegraph for body lasers
                float telegraphProgress = 0f;
                if (npc.TryGetAIOverride<DestroyerAI>(out var destroyerAI) && destroyerAI.LaserColor != -1)
                {
                    float shootProjectileTime = death ? (phase5 ? 180f : phase4 ? 270f : 360f) : 450f;
                    float telegraphGateValue = shootProjectileTime - DestroyerAI.LaserTelegraphTime;
                    if (calNPC.newAI[0] > telegraphGateValue)
                    {
                        switch (destroyerAI.LaserColor)
                        {
                            default:
                            case 0:
                                break;
                            case 1:
                                telegraphColor = telegraphColor_Green;
                                break;
                            case 2:
                                telegraphColor = telegraphColor_Cyan;
                                break;
                        }
                        telegraphProgress = MathHelper.Clamp((calNPC.newAI[0] - telegraphGateValue) / DestroyerAI.LaserTelegraphTime, 0f, 1f);
                    }
                }
                Color finalColor = Color.Lerp(segmentColor, telegraphColor, telegraphProgress);
                Vector3 teleHsl = Main.rgbToHsl(finalColor);

                Texture2D glowTexture = TextureAssets.Dest[npc.type - 134].Value;

                CalamityUtils.EnterShaderRegion(spriteBatch);
                GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["CalamityMod:BasicTint"].UseColor(Main.hslToRgb(1f - teleHsl.X, teleHsl.Y, teleHsl.Z));
                GameShaders.Misc["CalamityMod:BasicTint"].Apply();
                spriteBatch.Draw(glowTexture, npc.Center - screenPos + Vector2.UnitY * npc.gfxOffY, npc.frame, Color.White * npc.Opacity, npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
                CalamityUtils.ExitShaderRegion(spriteBatch);
            }
            return false;
        }
        return true;
    }

    private static void GlobalPostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var calNPC = npc.GetGlobalNPC<CalamityGlobalNPC>();
        bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
        bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

        // Energy shield
        if (npc.type == NPCID.CultistBoss || npc.type == NPCID.CultistBossClone)
        {
            spriteBatch.EnterShaderRegion();

            float intensity = calNPC.newAI[1] / 35f;
            float lifeRatio = npc.type == NPCID.CultistBoss ? (npc.life / (float)npc.lifeMax) : (Main.npc[(int)npc.ai[3]].life / (float)Main.npc[(int)npc.ai[3]].lifeMax);

            float flickerPower = 0f;
            if (lifeRatio < 0.85f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.7f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.55f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.4f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.25f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.1f)
                flickerPower += 0.1f;

            float opacity = 1f;
            opacity *= MathHelper.Lerp(MathHelper.Max(1f - flickerPower, 0.56f), 1f, (float)Math.Pow(Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.Lerp(3f, 5f, flickerPower)) * 0.5 + 0.5, 24D));

            // Dampen the opacity and intensity slightly, to allow Cultist to be more easily visible inside of the forcefield.
            // Dampen the opacity and intensity a bit more for the Clones.
            float intensityAndOpacityMult = npc.type == NPCID.CultistBossClone ? 0.9f : 1f;
            intensity *= intensityAndOpacityMult;
            opacity *= intensityAndOpacityMult;

            Texture2D forcefieldTexture = SupremeCalamitas.SupremeCalamitas.ForcefieldTexture.Value;

            if (npc.type == NPCID.CultistBoss)
                GameShaders.Misc["CalamityMod:SupremeShield"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            else
                GameShaders.Misc["CalamityMod:SupremeShield"].UseImage1("Images/Misc/noise");

            float colorTransitionAmt = (float)Math.Pow((double)(1f - lifeRatio), 2D);
            Color forcefieldColor = Color.Lerp(Color.MediumSpringGreen, Color.Black, colorTransitionAmt);
            Color secondaryForcefieldColor = Color.Lerp(Color.Cyan, Color.Blue, colorTransitionAmt);

            forcefieldColor *= opacity;
            secondaryForcefieldColor *= opacity;

            GameShaders.Misc["CalamityMod:SupremeShield"].UseSecondaryColor(secondaryForcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseColor(forcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseSaturation(1);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseOpacity(0.65f);
            GameShaders.Misc["CalamityMod:SupremeShield"].Apply();

            // Actual Cultist has a bigger shield than the Clones.
            float shieldScale = npc.type == NPCID.CultistBossClone ? 1.65f : MathHelper.Lerp(1.65f, 3f, (float)Math.Pow((double)lifeRatio, 2D));
            spriteBatch.Draw(forcefieldTexture, npc.Center - Main.screenPosition, null, Color.White * opacity, 0f, forcefieldTexture.Size() * 0.5f, shieldScale, SpriteEffects.None, 0f);

            spriteBatch.ExitShaderRegion();
        }

        // Laser telegraph
        else if (npc.type == NPCID.Probe)
        {
            float eyeTelegraphGateValue = (NPC.IsMechQueenUp ? DestroyerAI.ProbeLaserGateValue_Mechdusa : revenge ? DestroyerAI.ProbeLaserGateValue_Rev : DestroyerAI.ProbeLaserGateValue) - DestroyerAI.ProbeLaserTelegraphTime;
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/Sparkle").Value;
            Vector2 halfSize = npc.frame.Size() / 2;

            Vector2 drawPosition = npc.Center - screenPos + Vector2.UnitX.RotatedBy(npc.rotation) * (npc.width * 0.45f * npc.spriteDirection) + Vector2.UnitY * npc.gfxOffY;
            float colorScale = MathHelper.Clamp((npc.localAI[0] - eyeTelegraphGateValue) / DestroyerAI.ProbeLaserTelegraphTime, 0f, 1f);
            Color drawColor2 = new Color(255, 100, 150, 192) * colorScale;
            spriteBatch.SetBlendState(BlendState.Additive);
            spriteBatch.Draw(glowTexture, drawPosition, npc.frame, drawColor2, npc.rotation, halfSize, npc.scale * 1.1f, SpriteEffects.None, 0f);
            spriteBatch.SetBlendState(BlendState.AlphaBlend);
        }
    }
}
