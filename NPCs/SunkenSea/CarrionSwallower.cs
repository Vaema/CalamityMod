using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class CarrionSwallower : ModNPC
    {
        public ref float Timer => ref NPC.ai[0];
        public ref float AttackState => ref NPC.ai[1];
        public ref float ExtraTimer => ref NPC.ai[2];
        public ref float PusAmount => ref NPC.ai[3];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { PortraitPositionXOverride = 20f };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 42;
            NPC.lifeMax = 65;
            NPC.damage = 25;
            NPC.defense = 0;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.DD2_SkeletonHurt;
            NPC.DeathSound = SoundID.DD2_SkeletonDeath;
            NPC.noGravity = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<CarrionSwallowerBanner>();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
                [
                    new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.CarrionSwallower")
                ]);
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];
            NPC.spriteDirection = NPC.direction;

            // Passive when first spawned
            if (NPC.localAI[0] == 0f)
            {
                NPC.damage = 0;
                PusAmount = 3;
                NPC.velocity.X = 5f * NPC.direction;
                if (NPC.velocity.X == 0f)
                    NPC.direction = -NPC.direction;

                // Attack if get too close
                if (target.Distance(NPC.Center) < 320f)
                {
                    NPC.localAI[0] = 1f;
                    NPC.netUpdate = true;
                }
            }
            // Attacking
            else
            {
                NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
                Timer++;
                // Sac full
                if (AttackState == 0f)
                {
                    NPC.damage = 0;
                    if (Timer < 180f || (target.Distance(NPC.Center) > 400f && ExtraTimer == 0f))
                    {
                        Vector2 targetDirection = Utils.DirectionTo(NPC.Center, target.Center);
                        NPC.velocity = (NPC.velocity * 55f + targetDirection * 7f) / 56f;
                    }
                    else
                    {
                        ExtraTimer++;
                        NPC.velocity *= 0.975f;
                        // Fire pus at the player
                        if (ExtraTimer == 40f)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit10, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 spawn = NPC.Center + Vector2.UnitX * 13f * NPC.spriteDirection;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), spawn, Utils.DirectionTo(spawn, target.Center) * 10f, ModContent.ProjectileType<CarrionPus>(), 12, 0f, Main.myPlayer);
                            }

                            Timer = 0f;
                            ExtraTimer = 0f;
                            // Starts with 3 shots, after firing them all the sac empties and it switches to charge mode
                            PusAmount--;
                            if (PusAmount == 0)
                                AttackState = 1f;
                        }
                        else // Dust telegraph
                        {
                            Vector2 dustSpawn = NPC.Center + Vector2.UnitX * 20f * NPC.direction * NPC.scale;
                            Vector2 dustVel = (Vector2.UnitX * 3f).RotatedByRandom(MathHelper.Pi / 6f) * Main.rand.NextBool().ToDirectionInt();
                            Dust dust = Dust.NewDustPerfect(dustSpawn, ModContent.DustType<LightDust>(), dustVel, 100, Main.rand.NextBool() ? Color.Yellow : Color.DarkRed, 0.6f);
                            dust.noLightEmittence = true;
                        }
                    }
                }
                // Sac empty
                else
                {
                    if (Timer < 180f || (target.Distance(NPC.Center) > 320f && ExtraTimer == 0f))
                    {
                        Vector2 targetDirection = Utils.DirectionTo(NPC.Center, target.Center);
                        NPC.velocity = (NPC.velocity * 40f + targetDirection * 10f) / 41f;
                    }
                    else
                    {
                        ExtraTimer++;
                        // Charge
                        if (ExtraTimer == 40f)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit6, NPC.Center);
                            NPC.damage = NPC.defDamage;
                            NPC.velocity = Utils.DirectionTo(NPC.Center, target.Center) * 13.5f;

                            CustomPulse woosh = new(NPC.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/BloomRing", Vector2.One, 0f, 0.1f, 0.6f, 15);
                            GeneralParticleHandler.SpawnParticle(woosh);
                        }
                        else if (ExtraTimer < 40f)
                        {
                            Vector2 dustSpawn = NPC.Center + Main.rand.NextVector2CircularEdge(35f, 35f);
                            Vector2 dustVel = Utils.DirectionTo(dustSpawn, NPC.Center) * 4f + NPC.velocity * 0.75f;
                            Dust dust = Dust.NewDustPerfect(dustSpawn, ModContent.DustType<LightDust>(), dustVel, 100, Color.DarkRed, 0.6f);
                            dust.noLightEmittence = true;
                        }

                        NPC.velocity *= ExtraTimer > 60f ? 0.96f : ExtraTimer < 40f ? 0.98f : 1f;

                        // Reset
                        if (ExtraTimer == 70f)
                        {
                            NPC.damage = 0;
                            Timer = 0f;
                            ExtraTimer = 0f;
                            if (NPC.localAI[1] == 1f)
                            {
                                NPC.localAI[1] = 0f;
                                AttackState = 0f;
                                PusAmount = 3;
                            }
                        }
                    }
                }

                if (NPC.velocity.X == 0f)
                    NPC.velocity.X = -NPC.oldVelocity.X * 0.7f;
                if (NPC.velocity.Y == 0f)
                    NPC.velocity.Y = -NPC.oldVelocity.Y * 0.7f;
            }
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            // This code has to be run in ModifyHitPlayer because otherwise shields prevent it from working
            // We don't have to check the AttackState because it only deals contact damage while ramming
            // localAI[1] is used a temp set for filling with pus so that the current charge can finish before starting pus shooting mode
            NPC.localAI[1] = 1f;
            NPC.ForceNetUpdate();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneTimelessShores && !spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.Cavern.Chance * 0.4f;
            }
            return 0f;
        }

        /*public override void FindFrame(int frameHeight)
        {
            base.FindFrame(frameHeight);
        }*/

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            // If in the bestiary or is buffering the pus fill from hitting a charge, draw the sac; otherwise it's based on AttackState
            Rectangle frame = tex.Frame(2, 1, NPC.IsABestiaryIconDummy || NPC.localAI[1] == 1f ? 0 : (int)AttackState, NPC.frame.Y);

            spriteBatch.Draw(tex, NPC.Center - screenPos, frame, drawColor, NPC.rotation, frame.Size() / 2f, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            return false;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<WillOWisp>(), 1, 1, 2);
        }
    }
}
