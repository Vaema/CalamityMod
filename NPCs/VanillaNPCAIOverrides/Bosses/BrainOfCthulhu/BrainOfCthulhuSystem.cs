using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.DataStructures;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;

public class BrainOfCthulhuSystem : ModSystem
{
    public static bool IsBrainOfCthulhuTextureVanilla => _vanillaBoCTexture;
    private static bool _vanillaBoCTexture = true;
    internal static Asset<Texture2D> tendril;
    private static Texture2D tendrilGlow = null;
    private static Texture2D brainGlow = null;
    private static Texture2D creeperGlow = null;

    internal static float ScreenBlurStrength = 0f;

    internal static List<VerletSimulatedSegment>[] VerletTendrils = new List<VerletSimulatedSegment>[BrainOfCthulhuAI.GetBrainOfCthuluCreepersCountRevDeath()];

    private static int previousMusic = -1;
    public static int PreviousMusic => previousMusic;

    public override void OnModLoad()
    {
        if (!Main.dedServ)
            tendril = ModContent.Request<Texture2D>("Terraria/Images/Chain12");

        On_NPC.SpawnBoss += SpawnBrainNoMessage;
        On_Player.ItemCheck_UseBossSpawners += BlockRoar;
        On_Main.UpdateAudio_DecideOnNewMusic += StopBoss3FromStarting;
        On_Main.UpdateAudio_DecideOnTOWMusic += StopOWBoss1FromStarting;
    }

    private void StopBoss3FromStarting(On_Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
    {
        orig(self);

        if (NPC.crimsonBoss == -1 || !CalamityWorld.revenge || !Main.npc[NPC.crimsonBoss].active || !Main.npc[NPC.crimsonBoss].TryGetAIOverride<BrainOfCthulhuAI>(out var brainAI))
            return;

        if (previousMusic < 0 || previousMusic >= Main.musicFade.Length)
            return;

        // The last part leaves one frame at the end of the spawn animation where the boss music starts playing, so that it can be instantly maxed out
        if (brainAI.AIState < 0 && (brainAI.Time - Math.Abs(brainAI.SpawnTime) < 420))
        {
            if (Main.newMusic == MusicID.Boss3)
                Main.newMusic = previousMusic;

            if (Main.curMusic == MusicID.Boss3)
                Main.curMusic = previousMusic;
        }
    }

    private void StopOWBoss1FromStarting(On_Main.orig_UpdateAudio_DecideOnTOWMusic orig, Main self)
    {
        orig(self);

        if (NPC.crimsonBoss == -1 || !CalamityWorld.revenge)
            return;

        if (previousMusic < 0 || previousMusic >= Main.musicFade.Length)
            return;

        var brainAI = Main.npc[NPC.crimsonBoss].AIOverride<BrainOfCthulhuAI>();
        // The last part leaves one frame at the end of the spawn animation where the boss music starts playing, so that it can be instantly maxed out
        if (brainAI.AIState < 0 && (brainAI.Time - Math.Abs(brainAI.SpawnTime) < 420))
        {
            if (Main.newMusic == MusicID.OtherworldlyBoss1)
                Main.newMusic = previousMusic;

            if (Main.curMusic == MusicID.OtherworldlyBoss1)
                Main.curMusic = previousMusic;
        }
    }

    private void BlockRoar(On_Player.orig_ItemCheck_UseBossSpawners orig, Player self, int onWhichPlayer, Item sItem)
    {
        if (sItem.type != ItemID.BloodySpine || !CalamityWorld.revenge || !self.ItemTimeIsZero || self.itemAnimation <= 0)
        {
            orig(self, onWhichPlayer, sItem);
            return;
        }

        SoundEngine.PlaySound(SoundID.NPCDeath1, Main.LocalPlayer.Center);

        if (self.ZoneCrimson)
        {
            self.ApplyItemTime(sItem);
            CalamityUtils.SpawnBossUsingItem(self, NPCID.BrainofCthulhu);
        }
    }

    private void SpawnBrainNoMessage(On_NPC.orig_SpawnBoss orig, int spawnPositionX, int spawnPositionY, int Type, int targetPlayerIndex)
    {
        if (Type != NPCID.BrainofCthulhu || !CalamityWorld.revenge)
        {
            orig(spawnPositionX, spawnPositionY, Type, targetPlayerIndex);
            return;
        }

        int num = NPC.NewNPC(NPC.GetBossSpawnSource(targetPlayerIndex), spawnPositionX, spawnPositionY, Type, 1);

        if (num == 200 || num == -1)
            return;

        if (Main.player[targetPlayerIndex].HeldItem.type == ItemID.BloodySpine)
            BrainOfCthulhuAI.SummonedViaItem = true;

        NPC.crimsonBoss = num;
        Main.npc[num].target = targetPlayerIndex;
        Main.npc[num].timeLeft *= 20;

        previousMusic = Main.curMusic;

        if (Main.netMode == NetmodeID.Server && num < 200)
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num);
    }

    internal static Texture2D GetTendrilGlow()
    {
        if (tendrilGlow == null)
        {
            var tex = new Texture2D(Main.graphics.GraphicsDevice, tendril.Value.Width, tendril.Value.Height);

            var BaseArray = new Color[tex.Width * tex.Height];
            var ColorArray = new Color[tex.Width * tex.Height];
            tendril.Value.GetData(BaseArray);
            for (var i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
            }
            tex.SetData(ColorArray);
            tendrilGlow = tex;
        }

        return tendrilGlow;
    }

    internal static Texture2D GetBrainGlow()
    {
        if (brainGlow == null)
        {
            var tex = new Texture2D(Main.graphics.GraphicsDevice, TextureAssets.Npc[NPCID.BrainofCthulhu].Value.Width, TextureAssets.Npc[NPCID.BrainofCthulhu].Value.Height);

            var BaseArray = new Color[tex.Width * tex.Height];
            var ColorArray = new Color[tex.Width * tex.Height];
            TextureAssets.Npc[NPCID.BrainofCthulhu].Value.GetData(BaseArray);
            for (var i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
            }
            tex.SetData(ColorArray);
            brainGlow = tex;
        }
        return brainGlow;
    }

    internal static Texture2D GetCreeperGlow()
    {
        if (creeperGlow == null)
        {
            var tex = new Texture2D(Main.graphics.GraphicsDevice, TextureAssets.Npc[NPCID.Creeper].Value.Width, TextureAssets.Npc[NPCID.Creeper].Value.Height);

            var BaseArray = new Color[tex.Width * tex.Height];
            var ColorArray = new Color[tex.Width * tex.Height];
            TextureAssets.Npc[NPCID.Creeper].Value.GetData(BaseArray);
            for (var i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
            }
            tex.SetData(ColorArray);
            creeperGlow = tex;
        }
        return creeperGlow;
    }

    public override void OnWorldLoad()
    {
        if (!Main.dedServ)
        {
            Main.QueueMainThreadAction(() =>
            {
                var reference = ModContent.Request<Texture2D>("CalamityMod/NPCs/VanillaNPCAIOverrides/Bosses/BrainOfCthulhu/ReferenceBrainOfCthulhu", AssetRequestMode.ImmediateLoad).Value;

                Main.instance.LoadNPC(NPCID.BrainofCthulhu);
                var usedTexture = TextureAssets.Npc[NPCID.BrainofCthulhu].Value;

                int refSize = reference.Width * reference.Height;
                var referenceArray = new Color[refSize];
                reference.GetData(referenceArray);

                int texSize = usedTexture.Width * usedTexture.Height;
                var textureArray = new Color[texSize];
                usedTexture.GetData(textureArray);

                bool match = true;

                for (var i = 0; i < referenceArray.Length; i++)
                    if (referenceArray[i] != textureArray[i])
                    {
                        match = false;
                        break;
                    }

                _vanillaBoCTexture = match;
            });
        }
    }

    public override void PostUpdateNPCs()
    {
        if (!NPC.AnyNPCs(NPCID.BrainofCthulhu))
            BrainOfCthulhuAI.SummonedViaItem = false;

        if (Main.netMode != NetmodeID.Server)
        {
            if (NPC.crimsonBoss != -1 && CalamityWorld.revenge && Main.npc[NPC.crimsonBoss].ai[0] < (float)BrainOfCthulhuAI.BrainAIState.Phase2TransitionClosed)
            {
                List<NPC> creepers = Main.npc.Where(n => n.active && n.type == NPCID.Creeper).ToList();

                foreach (NPC creeper in creepers)
                {
                    int creeperID = (int)creeper.ai[0];

                    Vector2 startPoint = Main.npc[NPC.crimsonBoss].Center + Vector2.UnitY * 32;

                    float creeperRatio = creeperID / (float)BrainOfCthulhuAI.GetBrainOfCthuluCreepersCountRevDeath();
                    if (creeperID % 2 == 0)
                        startPoint += new Vector2(MathHelper.Lerp(-24, 0, creeperRatio), 0);
                    else
                        startPoint += new Vector2(MathHelper.Lerp(24, 0, creeperRatio), 0);

                    Vector2 endPoint = creeper.Center;

                    List<VerletSimulatedSegment> tendril = VerletTendrils[creeperID];
                    if (tendril is null || tendril.Count == 0)
                        continue;

                    tendril[0].position = startPoint;
                    tendril[0].locked = true;
                    tendril[^1].position = endPoint;
                    tendril[^1].locked = true;

                    VerletSimulatedSegment.SimpleSimulation(tendril, 16, 10, 3);
                }
            }
        }
    }

    public override void PostDrawTiles()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            if (NPC.crimsonBoss == -1 || !CalamityWorld.revenge || Main.npc[NPC.crimsonBoss].ai[0] >= (float)BrainOfCthulhuAI.BrainAIState.Phase2TransitionClosed)
            {
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseOpacity(0);
                if (Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].IsActive())
                    Filters.Scene.Deactivate("CalamityMod:BrainOfCthulhuForcefield");
            }
            else
            {
                if (!Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].IsActive())
                    Filters.Scene.Activate("CalamityMod:BrainOfCthulhuForcefield");

                NPC target = Main.npc[NPC.crimsonBoss];
                Vector2 targetPos = target.Center;
                float shieldOpacity = target.AIOverride<BrainOfCthulhuAI>().ShieldOpacity;
                float shieldScale = target.AIOverride<BrainOfCthulhuAI>().ShieldScale;
                targetPos = Vector2.Transform(targetPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.ScreenSize.ToVector2();

                Texture2D voronoi = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes3").Value;
                Texture2D depthNoise = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Veins").Value;

                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["voronoi"].SetValue(voronoi);
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["depthNoise"].SetValue(depthNoise);
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().Shader.Parameters["uScreenResolution"].SetValue(new Vector2(Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height));
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseProgress(0.15f * shieldScale);
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseOpacity(shieldOpacity);
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseColor(Color.Red);
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseSecondaryColor(new Color(255, 0, 90)); //Crimson color (R:220,G:20,B:60,A:255). // Magenta color (R:255,G:0,B:255,A:255).
                Filters.Scene["CalamityMod:BrainOfCthulhuForcefield"].GetShader().UseDirection(targetPos);
            }

            if (ScreenBlurStrength == 0f)
            {
                Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseIntensity(0);
                if (Filters.Scene["CalamityMod:RadialBlurShader"].IsActive())
                    Filters.Scene.Deactivate("CalamityMod:RadialBlurShader");
                return;
            }

            if (NPC.crimsonBoss == -1)
            {
                ScreenBlurStrength = Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().Intensity * 0.9f;
                Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseIntensity(ScreenBlurStrength);
                if (ScreenBlurStrength < 0.01f)
                    ScreenBlurStrength = 0f;
                return;
            }

            if (Filters.Scene["CalamityMod:RadialBlurShader"].IsLoaded)
            {
                if (!Filters.Scene["CalamityMod:RadialBlurShader"].IsActive())
                    Filters.Scene.Activate("CalamityMod:RadialBlurShader");
                NPC boss = Main.npc[NPC.crimsonBoss];
                float counter = boss.ai[1] - boss.ai[2] - 240;
                float distSQ = Main.LocalPlayer.DistanceSQ(boss.Center);
                float distanceScaleFactor = 1;
                if (distSQ > 592900) //770^2
                    distanceScaleFactor = 1 / (1 + (((float)Math.Sqrt(distSQ) - 770) / 32f));

                Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseIntensity((ScreenBlurStrength + (((float)Math.Cos(counter * MathHelper.TwoPi / 15f) / 2f + 0.5f) * (0.4f * ScreenBlurStrength))) * distanceScaleFactor);
                Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().Shader.Parameters["uSaturation"].SetValue(20);

                Vector2 targetPos = Vector2.Transform(boss.Center - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix) / Main.ScreenSize.ToVector2();

                Filters.Scene["CalamityMod:RadialBlurShader"].GetShader().UseDirection(targetPos);

            }
        }
    }

    public override void PostUpdateEverything()
    {
        if (Main.curMusic != MusicID.Boss3 && Main.curMusic != MusicID.OtherworldlyBoss1)
            previousMusic = Main.curMusic;

        if (previousMusic == -1)
            previousMusic = MusicID.Crimson;

    }
}

