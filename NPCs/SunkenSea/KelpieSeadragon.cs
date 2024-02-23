using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.World;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class KelpieSeadragon : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 13;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 20;
            NPC.width = 20;
            NPC.height = 58;
            NPC.defense = 5;
            NPC.lifeMax = 350;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.15f;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<KelpieSeadragonBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.KelpieSeadragon")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void AI()
        {
            if (NPC.direction == 0)
            {
                NPC.TargetClosest();
            }
            Player target = Main.player[NPC.target];
            switch (NPC.ai[0])
            {
                // Idle AI. Mostly sits still but occasionally moves in a random direction for a bit. 
                case 0:
                    NPC.chaseable = false;
                    if (target != null && target.active && target.Distance(NPC.Center) < 400 && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1))
                    {
                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                        NPC.TargetClosest();
                    }
                    if (NPC.velocity.Length() < 0.1f)
                    {
                        NPC.ai[1]++;
                        // Randomly switch direction
                        if (Main.rand.NextBool(120))
                        {
                            NPC.direction *= -1;
                        }
                        // Move in a random direction towards the direction the horse is facing
                        if (NPC.ai[1] > 120 || Main.rand.NextBool(90))
                        {
                            Vector2 direction = new Vector2(NPC.direction * 30, Main.rand.Next(-30, 30));
                            direction = direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 2;
                            NPC.ai[1] = 0;
                        }
                    }
                    // Reset any rotation from aggressive AI
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.1f);
                    NPC.velocity *= 0.99f;
                    break;
                case 1:
                    if (target == null || !target.active || target.Distance(NPC.Center) > 600)
                    {
                        NPC.ai[0] = 0;
                        NPC.ai[1] = 0;
                    }
                    NPC.chaseable = true;
                    // If the target is too far from its shooting range, move closer
                    if (target.Distance(NPC.Center) > 300)
                    {
                        NPC.ai[1] = 0;
                        NPC.velocity = NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 3f;
                        NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.direction * MathHelper.PiOver4 / 2, 0.05f);
                    }
                    else
                    {
                        // Otherwise sit at a distance and fire projectiles
                        NPC.velocity *= 0.9f;
                        NPC.ai[1]++;
                        if (NPC.ai[1] % 45 == 0)
                        {
                            SoundEngine.PlaySound(Sounds.CommonCalamitySounds.ExoPlasmaShootSound with { Volume = 0.2f, Pitch = 1.8f }, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 projSpeed = NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 6;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X, NPC.position.Y + 20), projSpeed, ModContent.ProjectileType<KelpDonut>(), NPC.damage, 0f);
                            }
                        }
                        NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.1f);
                    }
                    if (Math.Abs(NPC.velocity.X) > 0)
                    {
                        NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    }
                    else
                    {
                        NPC.direction = NPC.Center.X > target.Center.X ? -1 : 1;
                    }
                    break;
            }
            NPC.spriteDirection = NPC.direction;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            // Anger
            if (NPC.ai[0] == 1 && NPC.ai[1] > 0)
            {
                if (NPC.frame.Y > 12 * frameHeight || NPC.frame.Y < frameHeight * 7)
                {
                    NPC.frame.Y = frameHeight * 7;
                }
            }
            // Idle
            else
            {
                if (NPC.frame.Y > 6 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return null;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.9f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }
    }
}
