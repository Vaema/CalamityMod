using System;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideSeaSnail : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public ref float DustTimer => ref Projectile.localAI[0];
        public ref float CollisionTimer => ref Projectile.ai[0];
        public ref float PlayerStandStillTimer => ref Projectile.ai[1];
        public ref float PlayerFishingTimer => ref Projectile.ai[2];

        public const int FrameTime = 6; // Switches frame every 6 frames (10 FPS)
        public const int PeekCooldown = 150;

        public Player Owner => Main.player[Projectile.owner];
        public CalamityPlayer ModdedOwner => Owner.Calamity();

        // Extra requirements so you don't break the bobber, or snail, or how would you fish underwater??
        public bool StandingStill => Owner.velocity.Length() == 0 && !Owner.pulley && !Owner.mount.Active && !Collision.DrownCollision(Owner.position, Owner.width, Owner.height, Owner.gravDir);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 7;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 34;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!ModdedOwner.victideSnailSet)
            {
                for (int d = 0; d < 25; d++)
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, DustID.BubbleBurst_Purple, 0f, 0f, 0, default, 1f);
                    dust.velocity *= 2f;
                    dust.scale *= 1.15f;
                }
                Projectile.active = false;
                return;
            }

            if (Owner.dead)
                ModdedOwner.victideSnail = false;

            if (ModdedOwner.victideSnail)
                Projectile.timeLeft = 2;

            //Create a burst of dust as it gets created, teleported, or popped out
            DustTimer++;
            if (DustTimer <= 3)
            {
                for (int d = 0; d < 25; d++)
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, DustID.BubbleBurst_Purple, 0f, 0f, 0, default, 1f);
                    dust.velocity *= 2f;
                    dust.scale *= 1.15f;
                }
            }

            // Small amount of light
            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.5f);

            if (StandingStill)
            {
                PlayerStandStillTimer++;
                if (PlayerStandStillTimer >= PeekCooldown)
                {
                    // Pop out the shell when ready. This give a slight velocity boost outwards
                    if (PlayerStandStillTimer == PeekCooldown)
                    {
                        DustTimer = 0f;
                        Projectile.velocity = new Vector2(Main.rand.NextFloat(2f, 3.6f) * Owner.direction, Main.rand.NextFloat(-4f, 0f));
                    }

                    // Fall down into position
                    Projectile.velocity *= 0.98f;
                    Projectile.velocity.X *= 0.9f;
                    Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.1f, 14f);
                    Projectile.tileCollide = true;

                    bool? collide = CollidingWithLiquidOrPlatform(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                    if (collide.HasValue)
                    {
                        CollisionTimer++;
                        // Bobs up and down if collided with liquids
                        Projectile.velocity.Y = collide.Value ? MathF.Sin(CollisionTimer * 0.05f) * 0.25f : 0f;
                    }

                    Projectile.rotation = Projectile.rotation.AngleTowards(0, MathHelper.PiOver4 * 0.14f);

                    if (CollisionTimer > 0f)
                    {
                        Projectile.frameCounter++;
                        if (Projectile.frameCounter >= FrameTime)
                        {
                            Projectile.frameCounter = 0;
                            Projectile.frame = Math.Min(6, Projectile.frame + 1);
                        }
                    }
                }
                else // keep this guy spinnin'
                    Projectile.rotation += MathHelper.PiOver4 * 0.2f;
            }
            else
            {
                // Slowly puts its head back into shell
                if (Projectile.frame > 0)
                {
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= FrameTime)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame--;
                    }

                    if (Projectile.frame == 0)
                    {
                        CollisionTimer = 0f;
                        PlayerStandStillTimer = 0f;
                        PlayerFishingTimer = 0f;
                    }
                }
                // Return to resting position once shelled again
                else
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.tileCollide = false;

                    Vector2 desiredPosition = Owner.Center + Vector2.UnitY * (Owner.gfxOffY - 60f) * Owner.gravDir;
                    //Round the result so it doesn't jitter.
                    desiredPosition = new Vector2((int)desiredPosition.X, (int)desiredPosition.Y);
                    Projectile.Center = Projectile.Center.MoveTowards(desiredPosition, 7f + Owner.velocity.Length());
                    Projectile.rotation += MathHelper.PiOver4 * 0.2f;
                }
            }

            // Teleport and reset everything if too far away (this range is deliberately very short)
            if (Vector2.Distance(Projectile.Center, Owner.Center) > 240f)
            {
                Projectile.Center = Owner.Center + Vector2.UnitY * (Owner.gfxOffY - 60f) * Owner.gravDir;
                DustTimer = 0f;
                CollisionTimer = 0f;
                PlayerStandStillTimer = 0f;
                PlayerFishingTimer = 0f;
                Projectile.frame = 0;
                return;
            }

            // Only does fishing stuff when it's fully popped out
            if (Projectile.frame == 6)
            {
                // Give the player a grace period upon any input with the fishing rod
                // This means you can't just AFK with the fishing rod held
                if (Owner.itemAnimation > 0 && Owner.HeldItem.fishingPole > 0)
                    PlayerFishingTimer = 600f;
                else
                    PlayerFishingTimer = Math.Max(PlayerFishingTimer -1f, 0f);

                // Automatically casts bobbers one at a time
                // This casts the same way the pole does to prevent position variance
                int bobberType = ModContent.ProjectileType<VictideBobber>();
                if (Owner.ownedProjectileCounts[bobberType] < 1 && Owner.HeldItem.fishingPole > 0 && Projectile.owner == Main.myPlayer && PlayerFishingTimer > 0f)
                {
                    SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

                    Vector2 velocity = Owner.SafeDirectionTo(Main.MouseWorld) * Owner.HeldItem.shootSpeed;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, bobberType, 0, 0f, Projectile.owner, ai2: Projectile.whoAmI);
                }
            }
        }

        public static bool? CollidingWithLiquidOrPlatform(Vector2 Position, Vector2 Velocity, int Width, int Height)
        {
            Rectangle Hitbox = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Rectangle ProjectedHitbox = new Rectangle((int)(Position.X + Velocity.X), (int)(Position.Y + Velocity.Y), Width, Height);

            int xMin = Utils.Clamp((int)(Position.X / 16f) - 1, 0, Main.maxTilesX - 1);
            int xMax = Utils.Clamp((int)((Position.X + (float)Width) / 16f) + 2, 0, Main.maxTilesX - 1);
            int yMin = Utils.Clamp((int)(Position.Y / 16f) - 1, 0, Main.maxTilesY - 1);
            int yMax = Utils.Clamp((int)((Position.Y + (float)Height) / 16f) + 2, 0, Main.maxTilesY - 1);
            for (int i = xMin; i < xMax; i++)
            {
                for (int j = yMin; j < yMax; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile == null)
                        return null;

                    if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                    {
                        Rectangle Target = new Rectangle(i * 16, j * 16 , 16, 16);
                        if (Hitbox.Intersects(Target) || ProjectedHitbox.Intersects(Target))
                            return false;
                    }
                    if (tile.LiquidAmount > 0 && !tile.HasTile)
                    {
                        float LiquidDepth = (256 - tile.LiquidAmount) / 16f;
                        Rectangle Target = new Rectangle(i * 16, j * 16 + (int)LiquidDepth, 16, 16 - (int)LiquidDepth);
                        if (Hitbox.Intersects(Target))
                            return true;
                    }
                }
            }
            return null;
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            for (int d = 0; d < 45; d++)
            {
                Dust dust = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, DustID.BubbleBurst_Purple, 0f, 0f, 0, default, 1f);
                dust.velocity *= 2f;
                dust.scale *= 1.15f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Touches the ground
            if (oldVelocity.Y >= 0)
                CollisionTimer = 1f;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            bool CanPeekOut = PlayerStandStillTimer >= PeekCooldown || Projectile.frame > 0;
            Vector2 origin = !CanPeekOut ? new Vector2(15, 23) : frame.Size() * 0.5f;

            // Look at the player's direction
            SpriteEffects flip = Owner.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(tex, Projectile.Center + Vector2.UnitY * 2f - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, flip);

            // Fisbing Rod,,.
            if (Owner.ownedProjectileCounts[ModContent.ProjectileType<VictideBobber>()] > 0)
            {
                Texture2D rod = Terraria.GameContent.TextureAssets.Item[ModContent.ItemType<Cnidarian>()].Value;
                Main.EntitySpriteDraw(rod, Projectile.Center - Main.screenPosition + Vector2.UnitX * Owner.direction * 30f, null, lightColor, 0f, rod.Size() * 0.5f, Projectile.scale, flip);
            }
            return false;
        }
    }
}
