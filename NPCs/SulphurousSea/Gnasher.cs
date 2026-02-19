using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SulphurousSea
{
    public class Gnasher : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 25;
            NPC.width = 50;
            NPC.height = 36;
            NPC.defense = 30;
            NPC.lifeMax = 50;
            NPC.knockBackResist = 0.25f;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(copper: 60);
            NPC.HitSound = SoundID.NPCHit50;
            NPC.DeathSound = SoundID.NPCDeath54;
            NPC.chaseable = false;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<GnasherBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SulphurousSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Gnasher")
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
            float detectRange = Main.player[NPC.target].Calamity().GetAbyssAggro(120f);

            if (NPC.wet)
            {
                CalamityRegularEnemyAI.PassiveSwimmingAI(NPC, Mod, 0, detectRange, 0.15f, 0.1f, 6f, 4f, 0.1f, false);
            }
            else
            {
                NPC.noGravity = false;

                if (NPC.justHit || (!Main.player[NPC.target].dead && (Main.player[NPC.target].Center - NPC.Center).Length() < detectRange && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height)))
                    NPC.chaseable = true;

                float deceleration = 0.8f;

                if (NPC.chaseable && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && !Main.player[NPC.target].dead)
                {
                    NPC.TargetClosest();

                    NPC.spriteDirection = (NPC.direction > 0) ? -1 : 1;

                    float distanceFromTarget = MathHelper.Clamp((Main.player[NPC.target].Center - NPC.Center).Length() * 0.0025f, 0f, 1.5f);
                    float velocityMultiplier = CalamityWorld.death ? 1.2f : CalamityWorld.revenge ? 1f : 0.8f;
                    float maxVelocity = ((Main.expertMode ? 2.5f : 2.25f) - distanceFromTarget) * velocityMultiplier;
                    float accelerationX = CalamityWorld.death ? 0.7f : CalamityWorld.revenge ? 0.6f : 0.5f;

                    if (NPC.velocity.X < -maxVelocity || NPC.velocity.X > maxVelocity)
                    {
                        if (NPC.velocity.Y == 0f)
                            NPC.velocity *= deceleration;
                    }
                    else if (NPC.velocity.X < maxVelocity && NPC.direction == 1)
                    {
                        NPC.velocity.X += accelerationX;
                        if (NPC.velocity.X > maxVelocity)
                            NPC.velocity.X = maxVelocity;
                    }
                    else if (NPC.velocity.X > -maxVelocity && NPC.direction == -1)
                    {
                        NPC.velocity.X -= accelerationX;
                        if (NPC.velocity.X < -maxVelocity)
                            NPC.velocity.X = -maxVelocity;
                    }
                }
                else
                {
                    NPC.TargetClosest(false);

                    if (NPC.velocity != Vector2.Zero)
                    {
                        NPC.velocity *= deceleration;
                        if (NPC.velocity.Length() < 0.1f)
                            NPC.velocity = Vector2.Zero;
                    }
                }
            }
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
                return NPC.chaseable;

            return null;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity == Vector2.Zero)
            {
                NPC.frame.Y = 0;
                return;
            }

            NPC.frameCounter += NPC.chaseable ? 0.15f : 0.075f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<Irradiated>(), 120);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.PlayerSafe)
                return 0f;

            if (spawnInfo.Player.Calamity().ZoneSulphur)
                return 0.1f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<ContaminatedBile>(), 5);
            npcLoot.AddIf(() => Main.hardMode, ItemID.TurtleShell, 10);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);

                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Gnasher").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Gnasher2").Type, 1f);
                }
            }
        }
    }
}
