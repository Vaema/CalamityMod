using System;
using System.IO;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class Pigion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public CalamityPlayer moddedOwner => Owner.Calamity();
        public bool canDamage = false;
        public Color cl1 = Color.Gold;
        public Color cl2 = Color.Goldenrod;
        public bool clicked = false;
        public int hopTimer = 0;
        public int hopMax = 100;
        public bool hitGround = false;
        public bool hitWall = false;
        public Vector2 lastUnmodVel;
        public Vector2 mousePos;
        public int thrownTimer = 0;
        public int thrownTimerMax = 300;
        public bool deadPig = false;
        public ref float time => ref Projectile.ai[0];
        public ref float minionNumber => ref Projectile.ai[1];
        public bool pigGrabbed => Projectile.ai[2] == 5;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 21;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60 * Projectile.MaxUpdates;
            Projectile.ArmorPenetration = 25;
        }
        public void Frames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6 * Projectile.MaxUpdates)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 2)
                Projectile.frame = 0;
        }
        public override void AI()
        {
            mousePos = Owner.ClampedMouseWorld();
            if (time == 0)
            {
                time = Main.rand.Next(1, 800 + 1);
                Projectile.spriteDirection = Main.rand.NextBool() ? -1 : 1;
                lastUnmodVel = Projectile.velocity;
            }

            Frames();
            
            // Make an idle sound
            if (Main.rand.NextBool(280) && Projectile.soundDelay == 0)
            {
                SoundStyle oink = new("CalamityMod/Sounds/Item/Swine", 2);
                SoundEngine.PlaySound(oink with { Volume = 0.35f, Pitch = Main.rand.NextFloat(0.3f, 0.5f), MaxInstances = 5 }, Projectile.Center);
                Projectile.soundDelay = 180;
            }

            // Teleport back to player if too far away
            if (Projectile.Center.Distance(Owner.Center) > 1500)
            {
                Projectile.velocity = (Vector2.UnitX * 16).RotatedByRandom(MathHelper.TwoPi);
                Projectile.Center = Owner.Center;
            }

            // Check if being clicked and no other Pigions are being clicked
            if (Owner.controlUseItem && Projectile.Center.Distance(mousePos) <= 60 && !pigGrabbed && Projectile.ai[2] == 0)
                SetGrab(true);

            float hitboxSizeMult = 0.45f;
            bool touchingWall = Collision.SolidCollision(Vector2.Lerp(Projectile.TopLeft, Projectile.Left, 0.15f) + Vector2.UnitX * Projectile.velocity.X, 1, (int)(Projectile.height * hitboxSizeMult)) || Collision.SolidCollision(Vector2.Lerp(Projectile.TopRight, Projectile.Right, 0.15f) + Vector2.UnitX * Projectile.velocity.X, 1, (int)(Projectile.height * hitboxSizeMult));
            if (touchingWall)
            {
                FlipDirection(-Projectile.spriteDirection);
                Projectile.velocity.X += Projectile.spriteDirection;
            }
            if (!pigGrabbed)
                Slam(touchingWall, hitboxSizeMult);
            else
                thrownTimer = thrownTimerMax;
            lastUnmodVel = Projectile.velocity;

            Movement();

            // Push away enemies it touches
            NPC npc = Projectile.Center.ClosestNPCAt(40, false);
            if (npc != null && !pigGrabbed)
                npc.MoveNPC(Utils.DirectionTo(Projectile.Center, npc.Center), 6, true);

            if (thrownTimer > 0)
                thrownTimer--;
            hopTimer--;
            time++;
            Projectile.timeLeft++;
            if (moddedOwner.friendlyMinions < minionNumber || Owner.dead || deadPig)
            {
                Projectile.Kill();
                return;
            }
        }
        public void Slam(bool touchingWall, float hitboxSizeMult)
        {
            bool onPlatform = false;
            float thrownMult = Utils.Remap(thrownTimer, 0, thrownTimerMax, 0.6f, 1f);
            if (Projectile.velocity.Y > 0)
            {
                for (int i = 4; i < 8; i++)
                {
                    Point bottom = (Projectile.Bottom + Vector2.UnitY * i).ToTileCoordinates();
                    if (TileID.Sets.Platforms[CalamityUtils.ParanoidTileRetrieval(bottom.X, bottom.Y).TileType])
                    {
                        onPlatform = true;
                    }
                }
            }
            float velX = MathF.Abs(lastUnmodVel.X);
            float velY = MathF.Abs(lastUnmodVel.Y);
            float minMoveSpeed = 5;
            bool hitDown = velY > minMoveSpeed && (Collision.SolidCollision(Projectile.Bottom + Vector2.UnitY * Projectile.velocity.Y, (int)(Projectile.width * hitboxSizeMult), 1) || onPlatform);
            bool hitUp = velY > minMoveSpeed && Collision.SolidCollision(Projectile.Top + Vector2.UnitY * Projectile.velocity.Y, (int)(Projectile.width * hitboxSizeMult), 1);
            bool hitSide = velX > minMoveSpeed && touchingWall;
            bool hardHit = Projectile.velocity.Length() > 25;
            float volumeMult = Utils.GetLerpValue(minMoveSpeed - 2, minMoveSpeed * 5, Projectile.velocity.Length(), true);

            if (hitDown || hitUp)
            {
                if (!hitGround && MathF.Abs(lastUnmodVel.Y) > 1)
                {
                    if (hardHit)
                    {
                        deadPig = true;
                        return;
                    }
                    Projectile.velocity.Y = -lastUnmodVel.Y * thrownMult;
                    MakeDustAndSound(false, volumeMult);
                    hitGround = true;
                }
            }
            else
                hitGround = false;

            if (hitSide)
            {
                if (!hitWall && MathF.Abs(lastUnmodVel.X) > 1)
                {
                    if (hardHit)
                    {
                        deadPig = true;
                        return;
                    }
                    Projectile.velocity.X = -lastUnmodVel.X * thrownMult;
                    MakeDustAndSound(true, volumeMult);
                    hitWall = true;
                }
            }
            else
                hitWall = false;
        }
        public void MakeDustAndSound(bool wall, float volumeMult)
        {
            SoundStyle splat = new("CalamityMod/Sounds/NPCHit/PerfSmallHit", 3);
            SoundEngine.PlaySound(splat with { Volume = 0.8f * volumeMult, Pitch = Main.rand.NextFloat(0.3f, 0.4f) - volumeMult * 0.3f }, Projectile.Center);

            Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float power = 0.2f + Utils.GetLerpValue(0, 40, Projectile.velocity.Length());
            for (int i = 0; i < (int)(12 * power); i++)
            {
                float variance = Main.rand.NextFloat(-0.8f, 0.8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDustPixelated>(),
                    vel.RotatedBy(variance).RotatedByRandom(MathF.Abs(variance) / 3) * Main.rand.NextFloat(15.0f, 18.0f) * MathF.Pow((1 - MathF.Abs(variance)), 2), 0, default, power * Main.rand.NextFloat(2.3f, 3.2f) - MathF.Abs(variance));
                dust.noGravity = Main.rand.NextBool(2, 3);
                dust.color = Main.rand.NextBool() ? cl1 : cl2;
                dust.customData = new Vector2(0.8f, 1.1f);
                dust.fadeIn = 3.5f * power;
            }
        }
        public void FlipDirection(int newDirection)
        {
            Projectile.spriteDirection = newDirection;
            time = 1;
        }
        public void SetGrab(bool isGrabbed) => Projectile.ai[2] = (isGrabbed ? 5 : 0);
        public void Movement()
        {
            // Flip movement direction every once in a while
            if (time % 900 == 0)
                FlipDirection(-Projectile.spriteDirection);

            Projectile.velocity.X *= 0.97f;
            if (pigGrabbed)
            {
                FlipDirection(MathF.Sign(Projectile.Center.DirectionTo(mousePos).X));
                Vector2 goalVel = (mousePos - Projectile.Center) / (18);
                Projectile.velocity = goalVel;
            }
            if (!Owner.controlUseItem)
                SetGrab(false);

            if (Projectile.velocity.Y < 14)
                Projectile.velocity.Y += 0.3f * Utils.GetLerpValue(100, 0, hopTimer, true);

            int walkRate = 90; // Walk along slowly
            if (!pigGrabbed && time % walkRate == 0)
            {
                Projectile.velocity.X += 1 * Projectile.spriteDirection;
            }

            // lil hops when near other Pigions
            for (int x = 0; x < Main.maxProjectiles; x++)
            {
                Projectile projectile = Main.projectile[x];
                bool validPig = projectile.active && projectile.type == Projectile.type && projectile != Projectile;
                float distance = Vector2.Distance(Projectile.Center, projectile.Center);
                if (validPig)
                {
                    if (pigGrabbed)
                        projectile.ai[2] = 1; // set all other Pigions to be counted as not grabable
                    if (distance <= 80 && distance != 0 && hopTimer <= 0 && thrownTimer == 0)
                    {
                        int directionOfJump = -MathF.Sign(Projectile.Center.DirectionTo(projectile.Center).X);
                        Projectile.velocity += new Vector2(Main.rand.NextFloat(-2.5f, -4) * directionOfJump, -2);
                        FlipDirection(-directionOfJump);
                        hopTimer = 100;
                    }
                }
            }

            float minSpeed = 4;
            float maxSpeed = 14;
            float goalRot = Utils.AngleLerp(0, Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0), Utils.GetLerpValue(minSpeed, maxSpeed, Projectile.velocity.Length(), true));
            Projectile.rotation = goalRot;

        }
        public override void OnKill(int timeLeft)
        {
            int damage = (int)(Projectile.damage * MathF.Pow(Owner.GetBestClassDamage().ApplyTo(1), 0.3f + Owner.ownedProjectileCounts[Projectile.type] * 0.7f)); // Damage scales with number of pigions at 0.7 power
            float blastSize = 100;
            float minMultiplier = 0.1f;
            int hitsToMinMult = 6;
            Projectile blast = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(damage), -15, Owner.whoAmI, blastSize, minMultiplier, hitsToMinMult);
            blast.timeLeft = 5;
            blast.DamageType = AverageDamageClass.Instance;
            blast.CritChance = 100;

            Owner.SetScreenshake(3);
            SoundStyle die = new("CalamityMod/Sounds/Item/PigionSqueal");
            SoundEngine.PlaySound(die with { Volume = 0.55f, Pitch = Main.rand.NextFloat(-0.2f, 0.4f), MaxInstances = 7 }, Projectile.Center);
            SoundStyle die2 = new("CalamityMod/Sounds/NPCKilled/PerfMediumDeath");
            SoundEngine.PlaySound(die2 with { Volume = 0.6f, Pitch = Main.rand.NextFloat(0.2f, 0.3f) }, Projectile.Center);
            for (int i = 0; i < 24; i++)
            {
                Vector2 dustVel = (Vector2.One * 6).RotatedByRandom(100);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SquashDustPixelated>(), dustVel * Main.rand.NextFloat(0.1f, 0.8f));
                dust.noGravity = Main.rand.NextBool(2, 3);
                dust.scale = Main.rand.NextFloat(0.5f, 0.8f);
                dust.color = Color.Red;
                dust.noLightEmittence = true;
                dust.fadeIn = -0.4f;

                if (i % 2 == 0)
                {
                    Particle pulse1 = new CustomSpark(Projectile.Center, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(1, 6), "CalamityMod/Particles/BloomCircle", false, Main.rand.Next(20, 35 + 1), Main.rand.NextFloat(0.2f, 0.35f), Color.Red * 0.55f, new Vector2(1f, 1f), noShrink: true);
                    GeneralParticleHandler.SpawnParticle(pulse1);
                    pulse1.Pixelate = true;
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = (pigGrabbed && mousePos.Y > Projectile.Center.Y);
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            float minSpeed = 4;
            float maxSpeed = 20;
            Vector2 squash = new Vector2(Utils.Remap(Projectile.velocity.Length(), minSpeed, maxSpeed, 1, 2), Utils.Remap(Projectile.velocity.Length(), minSpeed, maxSpeed, 1, 0.5f));
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, squash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally :  SpriteEffects.None, 0);
            return false;
        }
        public override bool? CanDamage() => canDamage ? null : false;
        public override bool? CanCutTiles() => false;
    }
}
