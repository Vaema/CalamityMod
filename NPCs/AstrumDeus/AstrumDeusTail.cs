using System;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.AstrumDeus;

[HasPierceResist]
[LongDistanceNetSync(SyncWith = typeof(AstrumDeusHead))]
public class AstrumDeusTail : ModNPC
{
    public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.AstrumDeusHead.DisplayName");

    public static Asset<Texture2D> GlowTexture;
    public static Asset<Texture2D> GlowTexture2;
    public static Asset<Texture2D> TextureFlash;

    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.TrailingMode[Type] = 1;
        if (!Main.dedServ)
        {
            GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            GlowTexture2 = ModContent.Request<Texture2D>(Texture + "Glow2", AssetRequestMode.AsyncLoad);
            TextureFlash = ModContent.Request<Texture2D>(Texture + "GlowFlash", AssetRequestMode.AsyncLoad);
        }
    }

    public override void SetDefaults()
    {
        NPC.damage = 60; // 120
        NPC.npcSlots = 5f;
        NPC.width = 52;
        NPC.height = 68;
        NPC.defense = 50;
        NPC.DR_NERD(0.4f);
        NPC.LifeMaxNERB(200000, 240000, 650000);
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.knockBackResist = 0f;

        if (CalamityWorld.death || BossRushEvent.BossRushActive)
            NPC.scale *= 1.4f;
        else if (CalamityWorld.revenge)
            NPC.scale *= 1.35f;
        else if (Main.expertMode)
            NPC.scale *= 1.2f;

        NPC.alpha = 255;
        NPC.behindTiles = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = AstrumDeusHead.HitSound;
        NPC.DeathSound = AstrumDeusHead.DeathSound;
        NPC.netAlways = true;
        NPC.boss = true;
        NPC.dontCountMe = true;
        NPC.Calamity().VulnerableToHeat = true;
        NPC.Calamity().VulnerableToSickness = false;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(NPC.dontTakeDamage);
        for (int i = 0; i < 4; i++)
            writer.Write(NPC.Calamity().newAI[i]);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        NPC.dontTakeDamage = reader.ReadBoolean();
        for (int i = 0; i < 4; i++)
            NPC.Calamity().newAI[i] = reader.ReadSingle();
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        return false;
    }

    public override void AI()
    {
        CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

        // Difficulty variables
        bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
        bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
        bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

        // Deus cannot hit for 3 seconds or while invulnerable
        bool doNotDealDamage = calamityGlobalNPC.newAI[1] < 180f || NPC.dontTakeDamage;
        if (doNotDealDamage)
            NPC.damage = 0;
        else
            NPC.damage = NPC.defDamage;

        // Get a target
        if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();

        Player player = Main.player[NPC.target];

        bool increaseSpeed = Vector2.Distance(player.Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles;
        bool increaseSpeedMore = Vector2.Distance(player.Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles;

        // Inflict Extreme Gravity to nearby players
        if (revenge)
        {
            if (!Main.dedServ)
            {
                if (!Main.LocalPlayer.dead && Main.LocalPlayer.active && Vector2.Distance(Main.LocalPlayer.Center, NPC.Center) < CalamityGlobalNPC.CatchUpDistance350Tiles)
                    Main.LocalPlayer.AddBuff(ModContent.BuffType<DoGExtremeGravity>(), 2);
            }
        }

        // Life
        float lifeRatio = NPC.life / (float)NPC.lifeMax;

        // Phases based on life percentage
        bool halfHealth = lifeRatio < 0.5f;
        bool doubleWormPhase = calamityGlobalNPC.newAI[0] != 0f;
        bool startFlightPhase = lifeRatio < 0.8f || death || doubleWormPhase;
        bool phase2 = lifeRatio < 0.5f && doubleWormPhase && expertMode;
        bool phase3 = lifeRatio < 0.2f && doubleWormPhase && expertMode;
        bool splittingMines = lifeRatio < 0.7f;
        bool movingMines = lifeRatio < 0.3f && doubleWormPhase && expertMode;
        bool deathModeEnragePhase_Head = calamityGlobalNPC.newAI[0] == 3f;
        bool deathModeEnragePhase_BodyAndTail = false;

        // 5 seconds of resistance in phase 2, 10 seconds in phase 1, to prevent spawn killing
        float resistanceTime = doubleWormPhase ? 300f : 600f;

        calamityGlobalNPC.CurrentlyIncreasingDefenseOrDR = calamityGlobalNPC.newAI[1] < resistanceTime;

        // Flight timer
        float aiSwitchTimer = doubleWormPhase ? (Main.getGoodWorld ? 600f : 1200f) : (Main.getGoodWorld ? 900f : 1800f);

        calamityGlobalNPC.newAI[3] += 1f;
        if (calamityGlobalNPC.newAI[3] >= aiSwitchTimer)
            calamityGlobalNPC.newAI[3] = 0f;
        // Sound effect for swapping between attack behaviors in phase 2
        if (doubleWormPhase && calamityGlobalNPC.newAI[3] % aiSwitchTimer == 0f && !(deathModeEnragePhase_Head || deathModeEnragePhase_BodyAndTail))
            SoundEngine.PlaySound(AstrumDeusHead.SplitSound with { Pitch = -0.2f, Volume = 0.9f }, player.Center);

        // Phase for flying at the player
        bool flyAtTarget = calamityGlobalNPC.newAI[3] >= (aiSwitchTimer * 0.5f) && startFlightPhase;

        // Length of worms
        int phase1Length = death ? 80 : revenge ? 70 : expertMode ? 60 : 50;
        int phase2Length = death ? 40 : revenge ? 35 : expertMode ? 30 : 25;
        int gfbLength = death ? 8 : revenge ? 7 : expertMode ? 6 : 5;
        int maxLength = Main.zenithWorld && doubleWormPhase ? gfbLength : doubleWormPhase ? phase2Length : phase1Length;

        // Become gradually more pissed as more worms are killed
        int gfbMaxWormCount = 10;
        int gfbWormCount = 0;
        if (Main.zenithWorld)
            gfbWormCount = NPC.CountNPCS(ModContent.NPCType<AstrumDeusHead>());
        if (gfbWormCount > gfbMaxWormCount)
            gfbWormCount = gfbMaxWormCount;

        // Copy dontTakeDamage and Opacity from head
        NPC.dontTakeDamage = Main.npc[(int)NPC.ai[2]].dontTakeDamage;
        NPC.Opacity = Main.npc[(int)NPC.ai[2]].Opacity;
        deathModeEnragePhase_BodyAndTail = Main.npc[(int)NPC.ai[2]].Calamity().newAI[0] == 3f;
        if (deathModeEnragePhase_BodyAndTail)
        {
            NPC.defense = 25;
            calamityGlobalNPC.DR = 0.15f;
        }

        // Set worm variable
        if (NPC.ai[2] > 0f)
            NPC.realLife = (int)NPC.ai[2];

        // Alpha effects
        if (Main.npc[(int)NPC.ai[1]].alpha < 128 && !NPC.dontTakeDamage)
        {
            NPC.alpha -= 42;
            if (NPC.alpha < 0)
                NPC.alpha = 0;
        }

        // Check if other segments are still alive, if not, die
        bool shouldDespawn = true;
        int headType = ModContent.NPCType<AstrumDeusHead>();
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].type != headType || !Main.npc[i].active)
                continue;
            shouldDespawn = false;
            break;
        }
        if (shouldDespawn)
        {
            if (Main.npc.IndexInRange((int)NPC.ai[1]) && Main.npc[(int)NPC.ai[1]].active && Main.npc[(int)NPC.ai[1]].life > 0)
                shouldDespawn = false;
        }
        if (shouldDespawn)
        {
            NPC.life = 0;
            NPC.HitEffect(0, 10.0);
            NPC.checkDead();
            NPC.active = false;
            NPC.ForceNetUpdate(false);
        }

        // Direction
        if (NPC.velocity.X < 0f)
            NPC.spriteDirection = -1;
        else if (NPC.velocity.X > 0f)
            NPC.spriteDirection = 1;

        if (NPC.life > Main.npc[(int)NPC.ai[1]].life)
            NPC.life = Main.npc[(int)NPC.ai[1]].life;

        bool hasJustSpawned = calamityGlobalNPC.newAI[1] < resistanceTime * 0.4f && !doubleWormPhase; // Speed boost for the first 4 seconds after spawning
        float segmentVelocity = hasJustSpawned ? 25f : deathModeEnragePhase_Head ? 19f : death ? 17.5f : 16f;

        float segmentVelocityBoost = 5f * (1f - lifeRatio);
        segmentVelocity += segmentVelocityBoost;
        if (gfbWormCount > 0)
            segmentVelocity += (gfbMaxWormCount - gfbWormCount) * 0.444f;

        if (revenge)
        {
            float revMultiplier = 1.1f;
            segmentVelocity *= revMultiplier;
        }

        // Follow the head
        Vector2 segmentCenter = NPC.Center;
        float segmentTargetX = player.Center.X;
        float segmentTargetY = player.Center.Y;
        segmentTargetX = (int)(segmentTargetX / 16f) * 16;
        segmentTargetY = (int)(segmentTargetY / 16f) * 16;
        segmentCenter.X = (int)(segmentCenter.X / 16f) * 16;
        segmentCenter.Y = (int)(segmentCenter.Y / 16f) * 16;
        segmentTargetX -= segmentCenter.X;
        segmentTargetY -= segmentCenter.Y;

        if (NPC.ai[1] > 0f && NPC.ai[1] < Main.npc.Length)
        {
            try
            {
                segmentCenter = NPC.Center;
                segmentTargetX = Main.npc[(int)NPC.ai[1]].Center.X - segmentCenter.X;
                segmentTargetY = Main.npc[(int)NPC.ai[1]].Center.Y - segmentCenter.Y;
            }
            catch
            {
            }

            NPC.rotation = (float)Math.Atan2(segmentTargetY, segmentTargetX) + MathHelper.PiOver2;
            float segmentTargetDist = (float)Math.Sqrt(segmentTargetX * segmentTargetX + segmentTargetY * segmentTargetY);
            int segmentWidth = NPC.width;
            segmentTargetDist = (segmentTargetDist - segmentWidth) / segmentTargetDist;
            segmentTargetX *= segmentTargetDist;
            segmentTargetY *= segmentTargetDist;
            NPC.velocity = Vector2.Zero;
            NPC.position.X = NPC.position.X + segmentTargetX;
            NPC.position.Y = NPC.position.Y + segmentTargetY;

            if (segmentTargetX < 0f)
                NPC.spriteDirection = -1;
            else if (segmentTargetX > 0f)
                NPC.spriteDirection = 1;
        }

        // Play spawn sound on Deus on the first frame because otherwise the sound wouldn't play properly in multiplayer
        if (calamityGlobalNPC.newAI[1] == 0f && !doubleWormPhase)
        {
            SoundEngine.PlaySound(AstrumDeusHead.SpawnSound, NPC.Center);
            calamityGlobalNPC.newAI[1] = 1f;
        }

        // 5 seconds of resistance in phase 2, 10 seconds in phase 1, to prevent spawn killing
        if (calamityGlobalNPC.newAI[1] < resistanceTime && ((NPC.position - NPC.oldPosition).Length() > 2f || calamityGlobalNPC.newAI[1] > 1f))
            calamityGlobalNPC.newAI[1] += 1f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            return true;

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (NPC.spriteDirection == 1)
            spriteEffects = SpriteEffects.FlipHorizontally;

        bool deathModeEnragePhase = Main.npc[(int)NPC.ai[2]].Calamity().newAI[0] == 3f;
        bool doubleWormPhase = NPC.Calamity().newAI[0] != 0f && !deathModeEnragePhase;

        float cyanThreshold = Main.getGoodWorld ? 300f : 600f;
        // Tail is always the first segment to visually transition
        float transitionStart = cyanThreshold * 0.75f;
        float transitionEnd = cyanThreshold * 0.8f;
        bool drawCyan = NPC.Calamity().newAI[3] >= transitionEnd && NPC.Calamity().newAI[3] <= cyanThreshold + transitionEnd;
        bool inColorTrans = doubleWormPhase && NPC.Calamity().newAI[3] % cyanThreshold >= transitionStart && NPC.Calamity().newAI[3] % cyanThreshold <= transitionEnd;

        Texture2D wormTexture = TextureAssets.Npc[Type].Value;
        Texture2D otherTexture;
        Vector2 halfSizeTex = new Vector2(TextureAssets.Npc[Type].Value.Width / 2, TextureAssets.Npc[Type].Value.Height / 2);

        Vector2 drawLocation = NPC.Center - screenPos;
        drawLocation -= new Vector2(wormTexture.Width, wormTexture.Height) * NPC.scale / 2f;
        drawLocation += halfSizeTex * NPC.scale + new Vector2(0f, NPC.gfxOffY);
        spriteBatch.Draw(wormTexture, drawLocation, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTex, NPC.scale, spriteEffects, 0f);

        wormTexture = GlowTexture.Value;
        Color phaseColor = drawCyan ? Color.Cyan : Color.Orange;
        Color otherPhaseColor = drawCyan ? Color.Orange : Color.Cyan;
        if (doubleWormPhase) // otherTexture contains the opposite texture, and is faded in during the transition
        {
            wormTexture = drawCyan ? GlowTexture2.Value : wormTexture;
            otherTexture = drawCyan ? wormTexture : GlowTexture2.Value;
        }
        else
            otherTexture = wormTexture;

        Color wormColorLerp = Color.Lerp(Color.White, doubleWormPhase ? phaseColor : Color.Orange, 0.5f) * (deathModeEnragePhase ? 1f : NPC.Opacity);

        int timesToDraw = deathModeEnragePhase ? 3 : drawCyan ? 2 : 1;
        for (int i = 0; i < timesToDraw; i++)
        {
            float opacity = Utils.GetLerpValue(transitionStart, transitionEnd, NPC.Calamity().newAI[3] % cyanThreshold, true);
            spriteBatch.Draw(wormTexture, drawLocation, NPC.frame, wormColorLerp * (inColorTrans ? 1f - opacity : 1f), NPC.rotation, halfSizeTex, NPC.scale, spriteEffects, 0f);
            // Controls drawing the new fading in glowmask for the upcoming behavior
            if (inColorTrans)
                spriteBatch.Draw(otherTexture, drawLocation, NPC.frame, Color.Lerp(Color.White, otherPhaseColor, 0.5f) * opacity, NPC.rotation, halfSizeTex, NPC.scale, spriteEffects, 0f);
            // Controls drawing the white flash immediately after swapping behaviors
            if (doubleWormPhase && NPC.Calamity().newAI[3] % cyanThreshold < 25f)
                spriteBatch.Draw(TextureFlash.Value, drawLocation, NPC.frame, Color.White * MathHelper.Lerp(1f, 0f, NPC.Calamity().newAI[3] % cyanThreshold / 25f), NPC.rotation, halfSizeTex, NPC.scale, spriteEffects, 0f);
        }
        return false;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
    {
        return !NPC.dontTakeDamage;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0 && !Main.zenithWorld) // I value people's computers
        {
            NPC.position.X = NPC.position.X + (NPC.width / 2);
            NPC.position.Y = NPC.position.Y + (NPC.height / 2);
            NPC.width = 50;
            NPC.height = 50;
            NPC.position.X = NPC.position.X - (NPC.width / 2);
            NPC.position.Y = NPC.position.Y - (NPC.height / 2);
            for (int i = 0; i < 5; i++)
            {
                int purpleDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.PurpleCosmilite, 0f, 0f, 100, default, 2f);
                Main.dust[purpleDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[purpleDust].scale = 0.5f;
                    Main.dust[purpleDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 10; j++)
            {
                int astralDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 3f);
                Main.dust[astralDust].noGravity = true;
                Main.dust[astralDust].velocity *= 5f;
                astralDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<AstralOrange>(), 0f, 0f, 100, default, 2f);
                Main.dust[astralDust].velocity *= 2f;
            }
            if (!Main.dedServ)
            {
                float randomSpread = Main.rand.Next(-200, 201) / 100f;
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * randomSpread, Mod.Find<ModGore>("AstrumDeusTail1").Type, 1f);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity * randomSpread, Mod.Find<ModGore>("AstrumDeusTail2").Type, 1f);
            }
        }
    }

    public override bool CheckActive()
    {
        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        if (hurtInfo.Damage > 0)
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 180);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
    }
}
