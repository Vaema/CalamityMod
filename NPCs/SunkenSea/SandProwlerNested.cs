using System;
using CalamityMod.BiomeManagers;
using CalamityMod.DataStructures;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using CalamityMod.NPCs.Crags;
using CalamityMod.Enums;

namespace CalamityMod.NPCs.SunkenSea
{
    public class SandProwlerNested : SunkenSeaNPC
    {
        public bool PeekingOut;
        public bool HasChosenSpotToHideIn => SpotToHideIn != Vector2.Zero;
        public Point TileCoordsToHideIn
        {
            get => SpotToHideIn.ToTileCoordinates();
            set => SpotToHideIn = value.ToWorldCoordinates();
        }
        public Vector2 SpotToHideIn
        {
            get => new Vector2(NPC.ai[0], NPC.ai[1]);
            set
            {
                NPC.ai[0] = value.X;
                NPC.ai[1] = value.Y;
            }
        }

        public bool InHidingSpot => NPC.WithinRange(SpotToHideIn, 8f);

        public bool RetreatingToHidingSpot
        {
            get => NPC.ai[2] == 1f;
            set => NPC.ai[2] = value.ToInt();
        }

        public ref float SnapTimer => ref NPC.ai[3];
        public ref float SnapCooldown => ref NPC.localAI[0];
        public ref float InitialSnapDirection => ref NPC.localAI[1];
        public ref float CurrentSnapDirection => ref NPC.localAI[2];
        public override string Texture => "CalamityMod/NPCs/SunkenSea/SandProwler";
        protected override List<int> PreyIDs => new List<int>()
        {
            ModContent.NPCType<PolypPanasea>(),
            ModContent.NPCType<PrismaticGuppy>(),
            ModContent.NPCType<Slugbun>(),
        };

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<Polyperil>()
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.PolypForest;

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.TrailingMode[Type] = 0;
            NPCID.Sets.TrailCacheLength[Type] = 60;
            NPCID.Sets.UsesNewTargetting[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            Main.npcFrameCount[Type] = 11;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            NPC.damage = 50;
            NPC.width = 30;
            NPC.height = 30; //32
            NPC.defense = 10;
            NPC.lifeMax = 300;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 20);
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.chaseable = false;
            NPC.netAlways = true;
            Banner = ModContent.NPCType<SandProwler>();
            BannerItem = ModContent.ItemType<SandProwlerBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;

            NPC.waterMovementSpeed = 1f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.SeaSerpent")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(PeekingOut);
            writer.Write(SnapCooldown);
            writer.Write(InitialSnapDirection);
            writer.Write(CurrentSnapDirection);
            writer.Write(NPC.Calamity().newAI[1]);
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            PeekingOut = reader.ReadBoolean();
            SnapCooldown = reader.ReadSingle();
            InitialSnapDirection = reader.ReadSingle();
            CurrentSnapDirection = reader.ReadSingle();
            NPC.Calamity().newAI[1] = reader.ReadSingle();
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void AI()
        {
            // Choose an initial tile to hide in.
            if (Main.netMode != NetmodeID.MultiplayerClient && !HasChosenSpotToHideIn)
            {
                int tries;
                for (tries = 0; tries < 1800; tries++)
                {
                    int x = (int)(NPC.Center.X / 16f) + Main.rand.Next(-25, 25);
                    int y = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-25, 25);
                    Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);

                    // Try again if the tile isn't solid or isn't exposed to air.
                    if (!WorldGen.SolidTile(tile) || !CalamityUtils.IsTileExposedToAir(x, y, out float? angleToOpenAir))
                        continue;

                    // Try again if there's no open water near the tile.
                    Vector2 moveDirection = angleToOpenAir.Value.ToRotationVector2();
                    Vector2 collisionCheckPosition = new Vector2(x * 16f + 8f, y * 16f + 8f) + moveDirection * 16f;
                    float collisionDistance = CalamityUtils.DistanceToTileCollisionHit(collisionCheckPosition, moveDirection, 20) ?? 20;
                    if (collisionDistance <= 10)
                        continue;

                    TileCoordsToHideIn = new Point(x, y);
                    break;
                }

                // Just die if no spot was suitable.
                if (tries >= 1799)
                    NPC.active = false;

                NPC.Center = SpotToHideIn;
                NPC.netUpdate = true;
            }

            Entity target = null;

            // Look for coins
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item n = Main.item[i];
                if (n == null || !n.active || (n.type != ItemID.SilverCoin && n.type != ItemID.GoldCoin))
                    continue;
                // if its head touches the coin, eat it
                if (n.getRect().Intersects(NPC.getRect()))
                {
                    SoundEngine.PlaySound(SoundID.Item2 with { Pitch = 1.2f, Volume = 0.8f }, NPC.Center);
                    SoundEngine.PlaySound(SoundID.CoinPickup, NPC.Center);
                    int dustType = n.type == ItemID.SilverCoin ? DustID.SilverCoin : DustID.GoldCoin;
                    for (int j = 0; j < 4; j++)
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                    }
                    n.active = false;
                    break;
                }
                // Set the coin as the target
                if (n.Distance(NPC.Center) <= 300 && Collision.CheckAABBvLineCollision(NPC.Center, NPC.Size, NPC.Center, n.Center))
                {
                    target = n;
                }
            }
            // Go after sea minnows if not distracted by shiny coin
            if (target == null)
            {
                if (NPC.life > NPC.lifeMax * 0.99f)
                {
                    if (CurrentPrey != null)
                        target = CurrentPrey;
                }
                // If you've pissed it off, it now goes after YOU
                else
                {
                    NPC.TargetClosest(false);
                    target = Main.player[NPC.target];
                }
            }

            // Open mouth when launching towards a target
            if (SnapTimer > 0 && target != null && !PeekingOut)
            {
                NPC.Calamity().newAI[1] = 1;
            }
            else if (RetreatingToHidingSpot || InHidingSpot)
            {
                NPC.Calamity().newAI[1] = 0;
            }

            // Become invulnerable and mostly transparent if hiding in a tile.
            NPC.dontTakeDamage = InHidingSpot;
            NPC.Opacity = MathHelper.Clamp(NPC.Opacity - NPC.dontTakeDamage.ToDirectionInt(), 0.35f, 1f);

            // Also emit some particle effects as an indicator.
            if (InHidingSpot)
            {
                Dust sparkle = Dust.NewDustDirect(TileCoordsToHideIn.ToWorldCoordinates(0, 0), 16, 16, DustID.AncientLight);
                sparkle.color = Color.Orange;
                sparkle.velocity = Main.rand.NextVector2Circular(4f, 4f);
                sparkle.noGravity = true;
            }

            // Decide rotation.
            NPC.rotation = NPC.AngleFrom(SpotToHideIn) + MathHelper.PiOver2;

            // Prevent the tile from being destroyed.
            FixExploitManEaters.ProtectSpot(TileCoordsToHideIn.X, TileCoordsToHideIn.Y);

            // Do nothing other than hiding if instructed to do so.
            if (RetreatingToHidingSpot)
            {
                NPC.velocity = NPC.SafeDirectionTo(SpotToHideIn) * 0.002f;
                NPC.position += NPC.velocity.SafeNormalize(Vector2.Zero) * 5f;

                // Stop once the hiding spot has been reached.
                if (InHidingSpot)
                {
                    NPC.Center = SpotToHideIn;
                    NPC.velocity = Vector2.Zero;
                    RetreatingToHidingSpot = false;
                    NPC.netUpdate = true;
                }

                return;
            }

            // Don't do any snapping and such if the cooldown is active.
            if (SnapCooldown > 0f)
            {
                SnapCooldown--;
                if (SnapCooldown <= 0f)
                    NPC.netUpdate = true;
                return;
            }

            if (SnapTimer > 0f)
            {
                int snapTime = PeekingOut ? 45 : 32;
                float idealSpeed = PeekingOut ? 1.75f : 10f;
                float newSpeed = MathHelper.Lerp(NPC.velocity.Length(), idealSpeed, 0.08f);
                NPC.velocity = CurrentSnapDirection.ToRotationVector2() * newSpeed;

                // Get closer to the target if one not peeking.
                if (!PeekingOut && target != null)
                    CurrentSnapDirection = CurrentSnapDirection.AngleTowards(NPC.AngleTo(target.Center), 0.0125f);

                // Retreat if velocity is zero for some reason.
                if (NPC.velocity == Vector2.Zero)
                {
                    SnapTimer = 0f;
                    RetreatingToHidingSpot = true;
                    NPC.netUpdate = true;
                    return;
                }

                SnapTimer++;
                if (SnapTimer >= snapTime || (Collision.SolidCollision(NPC.Center, 1, 1) && SnapTimer > 5f))
                {
                    SnapTimer = 0f;
                    SnapCooldown = 35f;
                    PeekingOut = false;
                    RetreatingToHidingSpot = true;
                }

                return;
            }

            Vector2 snapDirection = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * Main.rand.Next(4));

            // Pick a potential direction to snap out.
            // This is important for attacking.
            int snapDirectionTries = 0;
            float targetSnapAngularThreshold = 0.48f;
            while ((CalamityUtils.DistanceToTileCollisionHit(SpotToHideIn, snapDirection, 50) ?? 50f) < 5f)
            {
                snapDirectionTries++;
                snapDirection = snapDirection.RotatedBy(MathHelper.PiOver2);

                if (snapDirectionTries >= 8)
                    return;

                // Try again if there's a defined target and it isn't in the line of sight of the current direction.
                if (target != null && snapDirection.AngleBetween(NPC.SafeDirectionTo(target.Center)) > targetSnapAngularThreshold)
                    continue;
            }

            // Snap out if a suitable target gets close.
            // Otherwise, sometimes randomly peek out.
            bool canSnapAtTarget =
                target != null &&
                snapDirection.AngleBetween(NPC.SafeDirectionTo(target.Center)) < targetSnapAngularThreshold &&
                Collision.CanHit(NPC.Center + snapDirection * 12f, 1, 1, target.Center, 1, 1);
            if (Main.rand.NextBool(30) && !canSnapAtTarget)
                PeekingOut = true;

            if (PeekingOut || canSnapAtTarget)
            {
                if (canSnapAtTarget)
                    SoundEngine.PlaySound(SoundID.Item96, NPC.Center);
                else
                    SoundEngine.PlaySound(SoundID.Item95, NPC.Center);

                // Add some randomness when peeking.
                if (PeekingOut)
                    snapDirection = snapDirection.RotatedByRandom(MathHelper.Pi / 6f);

                NPC.velocity = snapDirection * 4f;
                InitialSnapDirection = CurrentSnapDirection = NPC.velocity.ToRotation();
                SnapTimer = 1f;
                NPC.netUpdate = true;
            }
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            PlayerHurt();
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            PlayerHurt();
        }

        public void PlayerHurt()
        {
            NPC.chaseable = true;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.Calamity().newAI[1] == 1)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 3)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                }
                if (NPC.frame.Y < frameHeight * 5 || NPC.frame.Y > frameHeight * 10)
                {
                    NPC.frame.Y = frameHeight * 5;
                }
            }
            else
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (InHidingSpot)
                return false;

            Texture2D headTexture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D body1Texture = SandProwler.BodySprite1.Value;
            Texture2D body2Texture = SandProwler.BodySprite2.Value;
            Texture2D body3Texture = SandProwler.BodySprite3.Value;
            Texture2D body4Texture = SandProwler.BodySprite4.Value;

            Vector2 idealDrawPosition = SpotToHideIn;
            Vector2 backOffset = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * -18f;
            if (Collision.SolidCollision(idealDrawPosition + backOffset, 4, 4))
                idealDrawPosition += backOffset;

            List<Vector2> bezierPoints = new List<Vector2>()
            {
                idealDrawPosition
            };

            // Calculate points to create segments at based on a catmull-rom spine.
            float bendFactor = Utils.GetLerpValue(80f, 250f, NPC.Distance(idealDrawPosition), true);
            for (int i = 0; i < 20; i++)
            {
                Vector2 leftEnd = idealDrawPosition - InitialSnapDirection.ToRotationVector2() * bendFactor * 450f;
                Vector2 rightEnd = NPC.Center + CurrentSnapDirection.ToRotationVector2() * bendFactor * 450f;
                bezierPoints.Add(Vector2.CatmullRom(leftEnd, idealDrawPosition, NPC.Center, rightEnd, i / 19f));
            }
            bezierPoints.Add(NPC.Center);

            // And then generalize them with a bezier curve.
            BezierCurve bezierCurve = new BezierCurve(bezierPoints.ToArray());
            int totalChains = (int)(NPC.Distance(idealDrawPosition) / 16f);
            totalChains = (int)MathHelper.Clamp(totalChains, 2f, 100f);

            for (int i = 0; i < totalChains - 1; i++)
            {
                Texture2D textureToUse;


                if (i % 2 == 0)
                {
                    textureToUse = body3Texture;
                }
                else
                {
                    textureToUse = body4Texture;
                }

                switch (totalChains - i - 1)
                {
                    case 1:
                        textureToUse = headTexture;
                        break;
                    case 2:
                        textureToUse = body1Texture;
                        break;
                    case 3:
                        textureToUse = body2Texture;
                        break;
                }

                Vector2 drawPosition = bezierCurve.Evaluate(i / (float)totalChains);
                Color lightColor = Lighting.GetColor((int)(drawPosition.X / 16f), (int)(drawPosition.Y / 16f));
                float angle = (bezierCurve.Evaluate(i / (float)totalChains + 1f / totalChains) - drawPosition).ToRotation() + MathHelper.PiOver2;
                Rectangle frame = textureToUse.Frame(1, 1, 0, 0);
                if (textureToUse == headTexture)
                {
                    frame = NPC.frame;
                }
                Vector2 origin = new Vector2(textureToUse.Width / 2, textureToUse.Height / (textureToUse == headTexture ? 22 : 2));
                spriteBatch.Draw(textureToUse, drawPosition - Main.screenPosition, frame, lightColor, angle, origin, NPC.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
        protected override bool NPCSearchFilter(NPC n)
        {
            return Vector2.DistanceSquared(NPC.Center, n.Center) < 200f * 200f && (PreyIDs.Contains(n.type) || PredatorIDs.Contains(n.type));
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return NPC.life < NPC.lifeMax * 0.99f;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZonePolypForest && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity && !spawnInfo.PlayerSafe)
                return SpawnCondition.CaveJellyfish.Chance * 0.3f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => SandProwler.DefineSandProwlerLoot(npcLoot);

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0 && !Main.dedServ)
            {
                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
                }
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SeaSerpentGore1").Type, NPC.scale);
            }
        }
        public override void OnKill()
        {
            // Increase the kill count of Sand Prowlers for the Bestiary
            if (NPC.GetWereThereAnyInteractions())
            {
                NPC nPC = new NPC();
                nPC.SetDefaults(ModContent.NPCType<SandProwler>());
                Main.BestiaryTracker.Kills.RegisterKill(nPC);
            }
        }
    }
}
