using System;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.NPCs.NormalNPCs.IceClasper;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class KingSlimeJewelRuby : ModNPC
    {
        public static readonly SoundStyle ShatterSound = new SoundStyle("CalamityMod/Sounds/NPCKilled/CrownJewelShatter");
        public static readonly SoundStyle ShootSound = new SoundStyle("CalamityMod/Sounds/Custom/RedJewelFire");
        public static readonly SoundStyle ModeShiftSound = new SoundStyle("CalamityMod/Sounds/Custom/RedJewelModeShift");


        private const int BoltShootGateValue = 60;
        private const int BoltShootGateValue_Death = 60;
        private const float RubyLightTelegraphDuration = 45f;

        public static int JewelBoltDamage = 10; // 40

        private const int EmeraldChargePhaseGateValue = 120;
        private const int EmeraldChargeGateValue = 60;
        private const int EmeraldChargeGateValue_Death = 40;
        private const float EmeraldLightTelegraphDuration = 30f;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NeedsExpertScaling[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);

            NPCID.Sets.TrailingMode[NPC.type] = 7;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 24; // 72. Contact damage only occurs in the "Emerald" phase, which is Death exclusive.
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

            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                NPC.life = 0;
                OnKill();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            NPC kingSlime = null;
            foreach (NPC n in Main.npc)
            {
                if (n.active && n.type == NPCID.KingSlime)
                {
                    kingSlime = n;
                    break;
                }
            }

            // Invincibility logic
            if (death && kingSlime != null && kingSlime.active)
            {
                NPC.dontTakeDamage = true;
            }
            else
            {
                NPC.dontTakeDamage = false;
            }

            float kingSlimeLifeRatio = (kingSlime != null && kingSlime.active) ? (float)kingSlime.life / kingSlime.lifeMax : 1f;

            bool isInitialEmeraldPhase = death && (kingSlimeLifeRatio < 0.55f && kingSlimeLifeRatio >= 0.35f);
            bool isAlternatingPhase = death && kingSlimeLifeRatio < 0.35f;

            // [0]: 0f = ruby AI, 1f = emerald AI
            // [1]: Ruby attack counter
            // [2]: Emerald attack counter
            // [3]: Phase (0f = standard, 1f = initial Emerald, 2f = alternating)
            if (isInitialEmeraldPhase)
            {
                if (NPC.localAI[3] != 1f) // Transition to initial Emerald phase
                {
                    NPC.ai[0] = 0f; 
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.localAI[0] = 1f; // Become emerald
                    NPC.localAI[1] = 0; 
                    NPC.localAI[2] = 0;
                    NPC.localAI[3] = 1f; // Set overall phase flag to initial Emerald

                    SoundEngine.PlaySound(ModeShiftSound with { Volume = 0.5f });
                    NPC.netUpdate = true;

                    for (int i = 0; i < 6; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
                        GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Pink));
                    }
                }
            }

            else if (isAlternatingPhase)
            {
                if (NPC.localAI[3] != 2f) // Transition to alternating phase
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.localAI[0] = 0f; // Become ruby
                    NPC.localAI[1] = 0; 
                    NPC.localAI[2] = 0;
                    NPC.localAI[3] = 2f; // Flag as alternating

                    SoundEngine.PlaySound(ModeShiftSound with { Volume = 0.4f });
                    NPC.netUpdate = true;

                    for (int i = 0; i < 6; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
                        GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Pink));
                    }
                }
            }

            else // Either rev, or death when above 55%
            {
                if (NPC.localAI[3] != 0f) // Standard ruby AI w/o alternating
                {
                    NPC.ai[0] = 0f; 
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.localAI[0] = 0f; // Become ruby
                    NPC.localAI[1] = 0;
                    NPC.localAI[2] = 0;
                    NPC.localAI[3] = 0f; // Set overall phase flag to normal
                    NPC.netUpdate = true;
                }
            }

            bool currentModeIsEmerald = NPC.localAI[0] == 1f;

            if (currentModeIsEmerald)
            {
                Lighting.AddLight(NPC.Center, 0f, 0.8f, 0f); 

                if (NPC.ai[3] == 1f)
                {
                    NPC.knockBackResist = 0f;

                    if (NPC.ai[0] == 0f)
                    {
                        NPC.damage = 0;

                        // Slow down before dash, emit particles that home in toward the center
                        NPC.velocity *= 0.925f;
                        if (Main.rand.NextBool(3))
                        {
                            Vector2 dustVel2 = (Vector2.UnitX).RotatedByRandom(100) * Main.rand.NextFloat(9.5f, 13f);
                            Dust dust2 = Dust.NewDustPerfect(NPC.Center + dustVel2.SafeNormalize(Vector2.UnitX) * 150, ModContent.DustType<SquashDust>(), -dustVel2 * 1.15f, 0, default, Main.rand.NextFloat(0.9f, 1.2f));
                            dust2.noGravity = true;
                            dust2.fadeIn = 0.66f;
                            dust2.color = new(0, 200, 0);
                        }

                        NPC.ai[1] += 1f;

                        float anglularSpeed = NPC.ai[1] / EmeraldChargeGateValue_Death;
                        anglularSpeed = 0.1f + anglularSpeed * 0.4f;
                        NPC.rotation += anglularSpeed * NPC.direction;

                        if (NPC.ai[1] >= EmeraldChargeGateValue_Death)
                        {
                            for (int dusty = 0; dusty < 10; dusty++)
                            {
                                Vector2 dustVel = NPC.SafeDirectionTo(Main.player[NPC.target].Center + Main.player[NPC.target].velocity * 20f, -Vector2.UnitY) * Main.rand.NextFloat(-4f, -1f);
                                int emerald = Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
                                Main.dust[emerald].velocity = dustVel * Main.rand.NextFloat(1f, 2f);
                                Main.dust[emerald].noGravity = true;
                                if (Main.rand.NextBool())
                                {
                                    Main.dust[emerald].scale = 0.5f;
                                    Main.dust[emerald].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                                }
                            }
                            SoundEngine.PlaySound(SoundID.Item38, NPC.Center);

                            NPC.ai[0] = 1f;
                            NPC.ai[1] = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[0] == 1f)
                    {
                        NPC.damage = NPC.defDamage;

                        float chargeSpeed = 28f;
                        NPC.velocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center, -Vector2.UnitY) * chargeSpeed;
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

                        NPC.ai[0] = 2f;
                        NPC.ai[1] = 0f;
                        NPC.ForceNetUpdate();
                    }
                    else if (NPC.ai[0] == 2f)
                    {
                        NPC.damage = NPC.defDamage;

                        NPC.rotation += MathHelper.ToRadians(Math.Sign(NPC.velocity.X) * 15);

                        CustomSprite spriteParticle = new CustomSprite(NPC.Center - NPC.velocity, NPC.velocity * 0.8f, 10, "CalamityMod/NPCs/NormalNPCs/KingSlimeJewelEmerald", 1.2f, Color.DarkGreen.MultiplyRGBA(new Color(1f, 1f, 1f, 0f)));
                        spriteParticle.Rotation = NPC.rotation;
                        GeneralParticleHandler.SpawnParticle(spriteParticle);

                        NPC.ai[1] += 1f;
                        if (NPC.ai[1] >= EmeraldChargeGateValue_Death)
                        {
                            NPC.damage = 0;

                            for (int dusty = 0; dusty < 10; dusty++)
                            {
                                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                                int emerald = Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
                                Main.dust[emerald].velocity = dustVel * Main.rand.NextFloat(1f, 2f);
                                Main.dust[emerald].noGravity = true;
                                if (Main.rand.NextBool())
                                {
                                    Main.dust[emerald].scale = 0.5f;
                                    Main.dust[emerald].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                                }
                            }

                            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                            NPC.ai[0] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.ai[3] = 0f;
                            NPC.netUpdate = true;

                            NPC.velocity = Vector2.Zero;

                            if (isAlternatingPhase)
                            {
                                NPC.localAI[2]++;
                                if (NPC.localAI[2] >= 2)
                                {
                                    NPC.localAI[0] = 0f; // Become ruby
                                    NPC.localAI[1] = 0; // Reset ruby attack counter
                                    NPC.localAI[2] = 0; // Reset emerald attack counter

                                    SoundEngine.PlaySound(ModeShiftSound with { Volume = 0.5f });
                                    NPC.netUpdate = true;

                                    for (int i = 0; i < 6; i++)
                                    {
                                        GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
                                        GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Pink));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    NPC.damage = 0;

                    NPC.knockBackResist = 0.7f;

                    NPC.rotation = NPC.velocity.X / 15f;

                    float velocity = 5f;
                    float acceleration = 0.2f;

                    if (NPC.position.Y > Main.player[NPC.target].position.Y - 200f)
                    {
                        if (NPC.velocity.Y > 0f)
                            NPC.velocity.Y *= 0.98f;

                        NPC.velocity.Y -= acceleration;

                        if (NPC.velocity.Y > velocity)
                            NPC.velocity.Y = velocity;
                    }
                    else if (NPC.position.Y < Main.player[NPC.target].position.Y - 300f)
                    {
                        if (NPC.velocity.Y < 0f)
                            NPC.velocity.Y *= 0.98f;

                        NPC.velocity.Y += acceleration;

                        if (NPC.velocity.Y < -velocity)
                            NPC.velocity.Y = -velocity;
                    }

                    if (NPC.Center.X > Main.player[NPC.target].Center.X + 200f)
                    {
                        if (NPC.velocity.X > 0f)
                            NPC.velocity.X *= 0.98f;

                        NPC.velocity.X -= acceleration;

                        if (NPC.velocity.X > 8f)
                            NPC.velocity.X = 8f;
                    }
                    if (NPC.Center.X < Main.player[NPC.target].Center.X - 200f)
                    {
                        if (NPC.velocity.X < 0f)
                            NPC.velocity.X *= 0.98f;

                        NPC.velocity.X += acceleration;

                        if (NPC.velocity.X < -8f)
                            NPC.velocity.X = -8f;
                    }

                    NPC.ai[2] += 1f;
                    if (NPC.ai[2] >= EmeraldChargePhaseGateValue)
                    {
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 1f;
                        NPC.netUpdate = true;
                    }
                }
            }
            else // Ruby AI
            {
                Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

                NPC.rotation = NPC.velocity.X / 15f;

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
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), npcPos, projVector, type, JewelBoltDamage, 0f, Main.myPlayer);

                    NPC.netUpdate = true;

                    if (isAlternatingPhase)
                    {
                        NPC.localAI[1]++; 
                        if (NPC.localAI[1] >= 4)
                        {
                            NPC.localAI[0] = 1f; // Become emerald
                            NPC.localAI[1] = 0; // Reset ruby attack counter
                            NPC.localAI[2] = 0; // Reset emerald attack counter

                            SoundEngine.PlaySound(ModeShiftSound with { Volume = 0.5f });
                            NPC.netUpdate = true;

                            for (int i = 0; i < 6; i++)
                            {
                                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Red));
                                GeneralParticleHandler.SpawnParticle(new PointParticle(NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.5f, 1.5f), Color.Pink));
                            }
                        }
                    }
                }
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
            {
                GeneralParticleHandler.SpawnParticle(new CustomSprite(NPC.Center, new Vector2(0, -2).RotatedByRandom(start + MathHelper.ToRadians(20f)).RotatedBy(MathHelper.ToRadians(i * 125)), 120, "CalamityMod/Particles/KingSlimeRubyShards", 1f, new Color(255, 255, 255), Main.rand.NextFloat(0.2f, 0.6f), frameCount: 3, frame: i));

            }

            SoundEngine.PlaySound(ShatterSound, NPC.Center);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool emeraldMode = NPC.localAI[0] == 1f;

            Color col = emeraldMode ? Color.DarkOliveGreen : Color.Red;
            Color flashCol = emeraldMode ? Color.Lime : Color.Pink;

            float alph = 0f;
            float currentLightTelegraphDuration = emeraldMode ? EmeraldLightTelegraphDuration : RubyLightTelegraphDuration;
            float currentColorTelegraphGateValue = (emeraldMode ? EmeraldChargeGateValue_Death : BoltShootGateValue_Death) - currentLightTelegraphDuration;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>(emeraldMode ? "CalamityMod/NPCs/NormalNPCs/KingSlimeJewelEmerald" : Texture);
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/NPCs/NormalNPCs/KingSlimeJewelFlash");

            if (emeraldMode)
            {
                if (NPC.ai[0] == 0f && NPC.ai[1] > currentColorTelegraphGateValue)
                {
                    alph = MathHelper.Lerp(0f, 1f, (NPC.ai[1] - currentColorTelegraphGateValue) / currentLightTelegraphDuration);
                }

                if (NPC.ai[0] == 2f && CalamityClientConfig.Instance.Afterimages)
                {
                    for (int i = 1; i < NPC.oldPos.Length; i++)
                    {
                        Vector2 trailDrawPos = NPC.oldPos[i] + NPC.Size * 0.5f - screenPos;
                        Color trailColor = Color.Lime * (1f - (float)i / NPC.oldPos.Length) * 0.3f;
                        spriteBatch.Draw(tex.Value, trailDrawPos, null, trailColor, NPC.rotation, tex.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0f);
                    }
                }

            }
            else
            {
                if (NPC.ai[0] > currentColorTelegraphGateValue)
                    alph = MathHelper.Lerp(0f, 1f, (NPC.ai[0] - currentColorTelegraphGateValue) / currentLightTelegraphDuration);
            }

            Main.EntitySpriteDraw(tex.Value, NPC.Center - screenPos, tex.Frame(), Color.White, NPC.rotation, tex.Frame().Center(), 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(tex2.Value, NPC.Center - screenPos, tex2.Frame(), Color.Lerp(col, flashCol, alph).MultiplyRGBA(new Color(alph, alph, alph, 0f)), NPC.rotation, tex2.Frame().Center(), alph * 1.2f, SpriteEffects.None);

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            bool emeraldMode = NPC.localAI[0] == 1f;

            if (emeraldMode)
            {
                NPC.frameCounter = 2;
            }
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
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

            bool emeraldMode = NPC.localAI[0] == 1f;
            if (emeraldMode)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemEmerald, hit.HitDirection, -1f, 0, default, 1f);
                Main.dust[dust].noGravity = true;

                if (NPC.life <= 0)
                {
                    NPC.position = NPC.Center;
                    NPC.width = NPC.height = 45;
                    NPC.position.X = NPC.position.X - (NPC.width / 2);
                    NPC.position.Y = NPC.position.Y - (NPC.height / 2);

                    for (int i = 0; i < 2; i++)
                    {
                        int emeraldDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
                        Main.dust[emeraldDust].noGravity = true;
                        Main.dust[emeraldDust].velocity *= 3f;
                        if (Main.rand.NextBool())
                        {
                            Main.dust[emeraldDust].scale = 0.5f;
                            Main.dust[emeraldDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                        }
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        int emeraldDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemEmerald, 0f, 0f, 100, default, 3f);
                        Main.dust[emeraldDust2].noGravity = true;
                        Main.dust[emeraldDust2].velocity *= 5f;
                        emeraldDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
                        Main.dust[emeraldDust2].noGravity = true;
                        Main.dust[emeraldDust2].velocity *= 2f;
                    }
                }
            }
        }
    }
}

