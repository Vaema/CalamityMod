using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class StormlionSentry : ModNPC
    {
        public ref float time => ref NPC.ai[0];
        public ref float attackTimer => ref NPC.ai[1];
        public ref float jitter => ref NPC.ai[2];
        public ref float jitterRate => ref NPC.localAI[0];
        public ref float headRot => ref NPC.localAI[1];
        public ref float attackFeedback => ref NPC.localAI[2];
        public bool canBeMoved => NPC.localAI[3] == 0;
        public enum Mode
        {
            Idle,
            Agressive,
            Leaping,
            Digging
        }
        public Mode AIState
        {
            get => (Mode)(int)NPC.ai[3];
            set => NPC.ai[3] = (int)value;
        }

        public float fxFade = 1;
        public float fxFadeInv => attackFeedback - 1;
        public float targetRange = 830;
        public int attackRate = 180;
        public int digDir = 1;
        public Vector2 headPosition;
        public Vector2 attackPosition;

        public static Asset<Texture2D> headTexture;
        public static Asset<Texture2D> mandibleTexture;
        public static Asset<Texture2D> bodyTexture;

        public override void Load()
        {
            headTexture = ModContent.Request<Texture2D>(Texture + "Head");
            mandibleTexture = ModContent.Request<Texture2D>(Texture + "Mandible");
            bodyTexture = ModContent.Request<Texture2D>(Texture + "Body");
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { PortraitPositionXOverride = 20f };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 108;
            NPC.lifeMax = 60;
            NPC.damage = 15;
            NPC.defense = 3;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.HitSound = Effects.StormlionEffects.Hit;
            NPC.DeathSound = Effects.StormlionEffects.Killed;
            NPC.noGravity = true;
            NPC.noTileCollide = true; // Custom tile collision

            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };

            AIState = Mode.Idle;
            attackFeedback = 1;
            headRot = -MathHelper.PiOver2;

            Banner = NPC.type;
            BannerItem = ModContent.ItemType<StormlionSentryBanner>();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
                [
                    new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.StormlionSentry")
                ]);
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];
            NPC.spriteDirection = NPC.direction;

            bool npcFullyInTiles = Collision.SolidCollision(NPC.Center - Vector2.UnitY * 13, 15, 15);
            bool npcOnTiles = Collision.SolidCollision(NPC.Center + Vector2.UnitY * 10, 15, 15);
            bool targetInRange = Utils.Distance(target.Center, NPC.Center) <= targetRange && Collision.CanHit(headPosition, 1, 1, target.Center, 1, 1);

            attackFeedback = MathHelper.Lerp(attackFeedback, 1, 0.025f);
            fxFade = Utils.GetLerpValue(0, attackRate, attackTimer, true);

            if (!npcOnTiles)
            {
                if (NPC.velocity.Y < 0)
                    NPC.velocity.Y *= 0.97f;
                if (NPC.velocity.Y < 20)
                    NPC.velocity.Y += 0.5f;
                NPC.velocity.X *= 0.97f;
            }

            if (!npcFullyInTiles && npcOnTiles && AIState != Mode.Digging)
            {
                NPC.localAI[3] = 5;
                AIState = Mode.Digging;
            }
            if (AIState == Mode.Idle)
            {
                attackTimer = (int)MathHelper.Lerp(attackTimer, 0, 0.01f);
                jitterRate = MathHelper.Lerp(jitterRate, 0.1f, 0.05f);
                headRot = headRot.AngleLerp(-Vector2.UnitY.ToRotation(), 0.02f);
                if (targetInRange && !canBeMoved)
                    AIState = Mode.Agressive;

                if (Main.rand.NextBool(15) && time % 60 == 0)
                {
                    jitterRate = 4.7f;
                    SoundEngine.PlaySound((Main.rand.NextBool() ? Effects.StormlionEffects.Idle1 : Effects.StormlionEffects.Idle2) with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.2f, 0.1f) }, NPC.Center);
                }
            }
            if (AIState == Mode.Agressive)
            {
                if (!targetInRange && attackTimer >= 0)
                    AIState = Mode.Idle;

                if (attackTimer >= 0)
                {
                    jitterRate = MathHelper.Lerp(jitterRate, 1f + 3f * Utils.GetLerpValue(0, attackRate, attackTimer), 0.035f);
                    headRot = headRot.AngleLerp(NPC.Center.DirectionTo(target.Center).ToRotation(), 0.035f);
                }
                else
                    headRot = headRot.AngleLerp(NPC.Center.DirectionTo(target.Center).ToRotation(), 0.01f);

                if (attackTimer == attackRate)
                {
                    SoundEngine.PlaySound(Effects.StormlionEffects.Attack with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, NPC.Center);

                    Projectile.NewProjectile(NPC.GetSource_FromThis(), attackPosition, (headRot.ToRotationVector2() * 12), ModContent.ProjectileType<StormlionSentryBullet>(), 15, 0f, Main.myPlayer);
                    for (int i = 0; i <= 12; i++)
                    {
                        float variance = Main.rand.NextFloat(-0.6f, 0.6f);
                        int dustStyle = Effects.StormlionEffects.EnergyDust;
                        Dust dust2 = Dust.NewDustPerfect(attackPosition, dustStyle);
                        dust2.scale = Main.rand.NextFloat(1.5f, 1.7f) - Math.Abs(variance);
                        dust2.velocity = (headRot.ToRotationVector2() * 15).RotatedBy(variance) * Main.rand.NextFloat(0.3f, 1f) * (1 - Math.Abs(variance));
                        dust2.noGravity = true;
                        dust2.color = Effects.StormlionEffects.EnergyColor;
                    }

                    attackFeedback = 2;
                    attackTimer = -60;
                    jitterRate = 0;
                    NPC.ForceNetUpdate();
                }
                attackTimer++;
            }
            if (AIState == Mode.Digging)
            {
                if (time % 10 == 0)
                {
                    NPC.velocity = Vector2.Zero;
                    headRot = MathHelper.ToRadians(4 * digDir) - MathHelper.PiOver2;
                    SoundEngine.PlaySound(SoundID.WormDig with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, NPC.Center);
                    NPC.Center += Vector2.UnitY * 2;
                    digDir *= -1;
                }
                if (npcFullyInTiles)
                    AIState = Mode.Idle;
            }

            if (!canBeMoved)
                NPC.velocity = Vector2.Zero;


            bool dancing = false; // They will dance if a "music projectile" is on screen of if the player is using an instrument (except drums because that would be annoying to make work)
            for (int x = 0; x < Main.maxProjectiles; x++)
            {
                if (Main.mouseLeft && (target.HeldItem.type == ItemID.Bell || target.HeldItem.type == ItemID.CarbonGuitar || target.HeldItem.type == ItemID.IvyGuitar || target.HeldItem.type == ItemID.Harp || target.HeldItem.type == ItemID.TheAxe))
                {
                    dancing = true;
                }
                Projectile projectile = Main.projectile[x];
                if (!dancing && projectile.active && (projectile.type == ModContent.ProjectileType<AnahitasArpeggioNote>() || projectile.type == ModContent.ProjectileType<MelterNote1>() || projectile.type == ModContent.ProjectileType<MelterNote2>() || projectile.type == ModContent.ProjectileType<AmphibiansGuitarHoldout>() || projectile.type == ProjectileID.SparkleGuitar || projectile.type == ProjectileID.EighthNote || projectile.type == ProjectileID.QuarterNote || projectile.type == ProjectileID.TiedEighthNote))
                {
                    dancing = true;
                }
            }

            if (dancing)
            {
                float sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 14.37f / MathHelper.Pi);
                if (Main.zenithWorld)
                {
                    if (time % 2 == 0)
                    {
                        SoundEngine.PlaySound(Effects.StormlionEffects.Attack with { Volume = 0.2f, Pitch = 0.4f * sine, MaxInstances = 30 }, NPC.Center);
                        Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), attackPosition, (headRot.ToRotationVector2() * 6), ModContent.ProjectileType<StormlionSentryBullet>(), 15 * target.statLifeMax2, 0f, Main.myPlayer);
                        proj.friendly = true;
                        proj.hostile = false;
                    }
                    fxFade = 1;
                    attackTimer = attackRate * 0.9f;
                }
                else
                    attackTimer = 0;
                jitterRate = 1;
                NPC.dontTakeDamage = true;
                headRot = MathHelper.ToRadians(100 * sine) - MathHelper.PiOver2;
                AIState = Mode.Idle;
            }
            else
                NPC.dontTakeDamage = false;

            float orbSine = (float)Math.Sin(time * 0.575f / MathHelper.Pi) * Utils.GetLerpValue(attackRate * 1.1f, 0, attackTimer);
            headPosition = NPC.Center - (Vector2.UnitY * 27).RotatedBy(NPC.rotation) + headRot.ToRotationVector2() * 12 + NPC.velocity;
            attackPosition = headPosition + headRot.ToRotationVector2() * (25 + 8 * orbSine);
            NPC.rotation = Utils.AngleLerp(0, headRot + MathHelper.PiOver2, 0.35f);


            if (fxFade > 0)
            {
                Lighting.AddLight(attackPosition, Effects.StormlionEffects.EnergyColor.ToVector3() * 0.7f * fxFade);
            }

            time++;
            jitter += jitterRate;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, Effects.StormlionEffects.FleshDust, hit.HitDirection, -1f, 0, Effects.StormlionEffects.FleshColor, Main.rand.NextFloat(0.7f, 1.1f));
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 35; k++)
                {
                    bool type = !Main.rand.NextBool(3);
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, type ? Effects.StormlionEffects.EnergyDust : Effects.StormlionEffects.FleshDust, hit.HitDirection, -1f, 0, type ? Effects.StormlionEffects.EnergyColor : Effects.StormlionEffects.FleshColor, Main.rand.NextFloat(0.7f, 1.1f));
                }
                CalamityUtils.SpawnGores(NPC, "StormlionSentry", 3);
            }
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Vector2 spawnPosition = new Vector2(spawnInfo.SpawnTileX * 16 + 8, spawnInfo.SpawnTileY * 16);
            bool npcOnTiles = Collision.SolidCollision(spawnPosition + Vector2.UnitY * 50, 20, 20);

            if (spawnInfo.Player.Calamity().ZoneTimelessShores && !spawnInfo.Water && !spawnInfo.Player.Calamity().clamity && npcOnTiles)
            {
                return SpawnCondition.Cavern.Chance * 8f;
            }
            return 0f;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<StormlionMandible>());
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            if (NPC.IsABestiaryIconDummy)
            {
                spriteBatch.Draw(tex, NPC.Center - screenPos, null, drawColor, NPC.rotation, tex.Size() / 2f, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                return false;
            }
            if (time == 0)
                return false;

            Texture2D body = bodyTexture.Value;
            Texture2D head = headTexture.Value;
            Texture2D mandible = mandibleTexture.Value;
            Texture2D attack = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;

            spriteBatch.Draw(body, NPC.Center - screenPos, null, drawColor, NPC.rotation, body.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(head, headPosition - screenPos, null, drawColor, headRot + MathHelper.PiOver2, head.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
            for (int i = -1; i <= 1; i += 2)
            {
                float sine = (float)Math.Sin(jitter * 0.575f / MathHelper.Pi) * Utils.GetLerpValue(4.8f, 0, jitterRate);
                Vector2 madiblePos = headPosition + (Vector2.UnitX * 10 * i).RotatedBy(headRot + MathHelper.PiOver2);
                Vector2 madibleOrigin = (new Vector2(i == 1 ? 0 : mandible.Width, mandible.Height));
                float mandibleRot = MathHelper.ToRadians(5 * i) + MathHelper.Lerp(MathHelper.ToRadians(20) * sine * i, MathHelper.ToRadians(50) * i * fxFadeInv, fxFadeInv);
                spriteBatch.Draw(mandible, madiblePos - screenPos, null, drawColor, headRot + mandibleRot + MathHelper.PiOver2, madibleOrigin, NPC.scale, i == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            if (attackTimer > 0)
            {
                float fade = (float)Math.Pow(fxFade, 2);
                for (int i = 0; i < 3; i++)
                    spriteBatch.Draw(attack, attackPosition - screenPos, null, Color.Lerp(Effects.StormlionEffects.EnergyColor, Color.White, i * 0.3f) with { A = 0 } * fade, headRot, attack.Size() / 2f, (NPC.scale * 0.3f - (i * 0.08f)) * fade * Main.rand.NextFloat(0.8f, 1.1f), SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
