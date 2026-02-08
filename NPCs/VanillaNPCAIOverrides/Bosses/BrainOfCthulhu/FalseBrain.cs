using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;

[AutoloadBossHead]
public class FalseBrain : ModNPC, ILocalizedModType
{
    public new string LocalizationCategory => "NPCs";
    public override string BossHeadTexture => "Terraria/Images/NPC_Head_Boss_23";

    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults()
    {
        NPC.width = 160;
        NPC.height = 110;
        NPC.damage = 0;
        NPC.lifeMax = 1;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
        NPC.immortal = true;
        NPC.dontTakeDamage = true;
        NPC.chaseable = false;
        NPC.npcSlots = 0f;
        NPC.netAlways = true;
        NPC.ShowNameOnHover = false;
        Music = MusicID.Boss3;
        SceneEffectPriority = (SceneEffectPriority)(-1);

        NPC.localAI[0] = Main.rand.Next(6);

        NPC.localAI[1] = 1 + Main.rand.NextFloat(-0.25f, 0.25f);
    }
    private int Variant => (int)NPC.localAI[0];
    private float Angle => NPC.ai[0];
    private ref float Time => ref NPC.ai[1];
    internal bool BeenHit = false;
    int AttackTime = 0;
    int SpawnTime = 60;

    internal static float TimeDivisor => 360f;
    internal float DrawPriority => (float)Math.Cos(-Time * MathHelper.TwoPi / TimeDivisor);
    internal static List<int> blackListedProjectiles = [];

    public override void AI()
    {
        if (NPC.crimsonBoss == -1)
        {
            NPC.active = false;
            return;
        }

        NPC brain = Main.npc[NPC.crimsonBoss];
        NPC.GivenName = brain.GivenOrTypeName + $": {brain.life}/{brain.lifeMax}";

        if (brain.AIOverride<BrainOfCthulhuAI>().AttackFlag)
            BeenHit = true;

        if (BeenHit)
        {
            NPC.dontTakeDamage = true;

            if (AttackTime == 60)
                NPC.active = false;

            AttackTime++;
        }
        else
        {
            float lerp = CalamityUtils.SineInOutEasing(MathHelper.Clamp(SpawnTime / -30f, 0f, 1f), 1);
            float baseDist = 240;
            float circleDist = 480;
            if (SpawnTime != -30)
            {
                baseDist = MathHelper.Lerp(480, 240, lerp);
                circleDist = MathHelper.Lerp(240, 480, lerp);
            }
            NPC.Center = brain.AIOverride<BrainOfCthulhuAI>().AttackPosition + Vector2.UnitX.RotatedBy(Angle) * (baseDist + (circleDist * ((float)Math.Sin(-Time * MathHelper.TwoPi / TimeDivisor) / 2f + 0.5f)));
            NPC.Center += Vector2.UnitX.RotatedBy(Angle + MathHelper.PiOver2) * (90 * (float)Math.Cos(Time * MathHelper.TwoPi / TimeDivisor));
        }

        if (SpawnTime > 0)
        {
            if (--SpawnTime == 0)
                NPC.dontTakeDamage = false;
        }
        else if (SpawnTime > -30)
            Time += --SpawnTime / -30f;
        else
            Time++;
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        SmiteFool(player);
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (!projectile.Calamity().IgnoreBoCIllusions && projectile.owner != -1)
            SmiteFool(Main.player[projectile.owner]);
    }

    private void SmiteFool(Player fool)
    {
        if (!BeenHit)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 dir = fool.Center - NPC.Center;
                int lifeTime = 24;
                dir /= lifeTime / 2f * 5f;
                dir *= i;
                DirectionalPulseRing pulse = new(NPC.Center, dir, i % 2 == 0 ? Color.Red : Color.Orange, new Vector2(0.5f, 1), dir.ToRotation(), 0f, i / 5f, lifeTime + 8);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            BeenHit = true;
            SoundEngine.PlaySound(BrainOfCthulhuAI.Laugh, NPC.Center);
            fool.AddBuff(BuffID.Darkness, 900);
            fool.AddBuff(BuffID.Bleeding, 900);
            fool.AddBuff(BuffID.Confused, 60);
            int timeToAdd = 600;
            int bbIndex = fool.buffType.ToList().IndexOf(ModContent.BuffType<BurningBlood>());
            if (bbIndex != -1)
            {
                timeToAdd /= 2;
                timeToAdd += fool.buffTime[bbIndex];
            }
            fool.AddBuff(ModContent.BuffType<BurningBlood>(), timeToAdd);

            fool.Calamity().adrenaline = 0;

            NPC.dontTakeDamage = true;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 0.2f * NPC.localAI[1];
        if ((int)NPC.frameCounter > 3)
            NPC.frameCounter = 0;
    }

    internal void DrawSelf(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex;
        Rectangle frame;

        Vector2 scaleDistort = new Vector2((float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f);
        Vector2 scaleAddition = Vector2.zeroVector;
        float endLerp = AttackTime / 60f;
        float startLerp = SpawnTime / 60f;

        if (BrainOfCthulhuSystem.IsBrainOfCthulhuTextureVanilla)
        {
            tex = TextureAssets.Npc[Type].Value;
            frame = tex.Frame(7, 4, Variant, (int)NPC.frameCounter);
        }
        else
        {
            tex = TextureAssets.Npc[NPCID.BrainofCthulhu].Value;
            frame = tex.Frame(1, 8, 0, (int)NPC.frameCounter);
        }

        if (SpawnTime > 0)
        {
            drawColor *= (1 - startLerp);
            scaleDistort *= startLerp;
        }
        else if (AttackTime > 0)
        {
            drawColor = Color.Lerp(drawColor, Color.Red, endLerp) * (1 - endLerp);
            scaleDistort *= endLerp;
        }
        else
            scaleDistort = Vector2.Zero;

        spriteBatch.Draw(tex, NPC.Center + (Vector2.UnitY * 16) - screenPos, frame, drawColor, NPC.rotation, frame.Size() * 0.5f, (Vector2.One + scaleAddition + scaleDistort) * NPC.scale, 0, 0);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
}
