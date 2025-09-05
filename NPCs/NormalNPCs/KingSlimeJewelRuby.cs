using System.Collections.Generic;
using System.Runtime.InteropServices;
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
using static System.Net.Mime.MediaTypeNames;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class KingSlimeJewelRuby : ModNPC
    {
        public static readonly SoundStyle ShatterSound = new SoundStyle("CalamityMod/Sounds/NPCKilled/CrownJewelShatter");
        public static readonly SoundStyle ShootSound = new SoundStyle("CalamityMod/Sounds/Custom/RedJewelFire");

        private const int BoltShootGateValue = 60;
        private const int BoltShootGateValue_Death = 75;
        private const float LightTelegraphDuration = 45f;

        public static int JewelBoltDamage = 10; // 40

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
            NPC.defense = 10;
            NPC.DR_NERD(0.1f);
            NPC.lifeMax = 120;
            NPC.knockBackResist = 0.8f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath15;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void AI()
        {
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Despawn
            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                NPC.life = 0;
                OnKill();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

            // Float around the player
            NPC.rotation = NPC.velocity.X / 15f;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                CalamityUtils.CalamityTargeting(NPC, default);
            }

            float velocity = 5f;
            float acceleration = 0.1f;

            if (NPC.position.Y > Main.player[NPC.target].position.Y - 350f)
            {
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= 0.98f;

                NPC.velocity.Y -= acceleration;

                if (NPC.velocity.Y > velocity)
                    NPC.velocity.Y = velocity;
            }
            else if (NPC.position.Y < Main.player[NPC.target].position.Y - 450f)
            {
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y *= 0.98f;

                NPC.velocity.Y += acceleration;

                if (NPC.velocity.Y < -velocity)
                    NPC.velocity.Y = -velocity;
            }

            if (NPC.Center.X > Main.player[NPC.target].Center.X + 100f)
            {
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X *= 0.98f;

                NPC.velocity.X -= acceleration;

                if (NPC.velocity.X > 8f)
                    NPC.velocity.X = 8f;
            }
            if (NPC.Center.X < Main.player[NPC.target].Center.X - 100f)
            {
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X *= 0.98f;

                NPC.velocity.X += acceleration;

                if (NPC.velocity.X < -8f)
                    NPC.velocity.X = -8f;
            }

            // Fire projectiles
            NPC.ai[0] += 1f;
            if (NPC.ai[0] >= (death ? BoltShootGateValue_Death : BoltShootGateValue))
            {
                NPC.ai[0] = 0f;

                Vector2 npcPos = NPC.Center;
                float xDist = Main.player[NPC.target].Center.X - npcPos.X;
                float yDist = Main.player[NPC.target].Center.Y - npcPos.Y;
                Vector2 projVector = new Vector2(xDist, yDist);
                float projLength = projVector.Length();

                float speed = death ? 12f : 10f;
                int type = ModContent.ProjectileType<JewelProjectile>();

                projLength = speed / projLength;
                projVector.X *= projLength;
                projVector.Y *= projLength;

                for (int i = 0; i < 6; i++)
                {
                    GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
                    GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Pink));
                }

                SoundEngine.PlaySound(ShootSound, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (death)
                    {
                        int numProj = 4;
                        float rotation = MathHelper.ToRadians(18);
                        for (int i = 0; i < numProj; i++)
                        {
                            Vector2 perturbedSpeed = projVector.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), npcPos, perturbedSpeed, type, JewelBoltDamage, 0f, Main.myPlayer);
                        }
                    }
                    else
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), npcPos, projVector, type, JewelBoltDamage, 0f, Main.myPlayer);
                }

                NPC.netUpdate = true;
            }
        }

        public override void OnKill()
        {
            for (int i = 0; i < 6; i++)
            {
                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Pink));
            }

            float start = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 3; i++)
                GeneralParticleHandler.SpawnParticle(new CustomSprite(NPC.Center, new Vector2(0, -2).RotatedByRandom(start + MathHelper.ToRadians(20f)).RotatedBy(MathHelper.ToRadians(i * 125)), 120, "CalamityMod/Particles/KingSlimeRubyShards", 1f, new Color(255, 255, 255), Main.rand.NextFloat(0.2f, 0.6f), frameCount: 3, frame: i));

            SoundEngine.PlaySound(ShatterSound, NPC.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Color col = Color.Red;
            Color flashCol = Color.Pink;

            float alph = 0f;

            float colorTelegraphGateValue = (CalamityWorld.death ? BoltShootGateValue_Death : BoltShootGateValue) - LightTelegraphDuration;

            if (NPC.ai[0] > colorTelegraphGateValue)
                alph = MathHelper.Lerp(0f, 1f, (NPC.ai[0] - colorTelegraphGateValue) / LightTelegraphDuration);

            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/NPCs/NormalNPCs/KingSlimeJewelFlash");

            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos, tex.Frame(), Color.White, NPC.rotation, tex.Frame().Center(), 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(tex2.Value, NPC.Center - screenPos, tex2.Frame(), Color.Lerp(col, flashCol, alph).MultiplyRGBA(new Color(alph, alph, alph, 0f)), NPC.rotation, tex2.Frame().Center(), alph * 1.2f, SpriteEffects.None);

            return false;
        }

        public override Color? GetAlpha(Color drawColor) => Color.White;

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override bool CheckActive() => false;

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 6; i++)
                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
        }
    }
}
