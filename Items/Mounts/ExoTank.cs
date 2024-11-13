using System;
using System.Collections.Generic;
using CalamityMod.Buffs.Mounts;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class ExoTank : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.spawnDust = DustID.Iron;
            MountData.spawnDustNoGravity = true;
            MountData.buff = ModContent.BuffType<ExoTankBuff>();

            // Horizontal movement
            MountData.runSpeed = 9f;
            MountData.dashSpeed = 22.5f;
            MountData.swimSpeed = 9f;
            MountData.acceleration = 0.5f;

            // Vertical movement
            MountData.fallDamage = 0f;
            MountData.jumpHeight = 10;
            MountData.jumpSpeed = 12f;

            // Frames and offsets
            MountData.totalFrames = 3;
            MountData.heightBoost = 80;
            int[] array = new int[MountData.totalFrames];
            for (int l = 0; l < array.Length; l++)
                array[l] = 76;

            MountData.playerYOffsets = array;
            MountData.playerHeadOffset = 0;
            MountData.bodyFrame = 3;
            MountData.xOffset = 0;
            MountData.yOffset = -16;
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            MountData.runningFrameCount = 3;
            MountData.runningFrameDelay = 24;
            MountData.runningFrameStart = 0;
            MountData.flyingFrameCount = 0;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 0;
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = MountData.standingFrameDelay;
            MountData.inAirFrameStart = 0;
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = MountData.standingFrameDelay;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;
            MountData.swimFrameCount = MountData.inAirFrameCount;
            MountData.swimFrameDelay = MountData.inAirFrameDelay;
            MountData.swimFrameStart = MountData.inAirFrameStart;
            if (Main.netMode != NetmodeID.Server)
            {
                MountData.backTextureExtra = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTankExtra");
                MountData.backTextureExtraGlow = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTankExtraGlow");
                MountData.frontTextureGlow = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTank_FrontGlow");
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }

        public override void UpdateEffects(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Mount tank = player.mount;

                // Find the closest NPC targetable by the tank
                float range = 960f; // 60 tiles
                int targetNPC = -1;
                foreach (NPC target in Main.ActiveNPCs)
                {
                    if (!target.CanBeChasedBy(tank))
                        continue;

                    Vector2 targetDif = target.Center - player.Center;

                    // The tank has a sight line of 120 degrees, similar to Toy Tank
                    if (player.direction == 1 && MathF.Abs(targetDif.ToRotation()) > MathHelper.ToRadians(60f))
                        continue;

                    if (player.direction == -1 && MathF.Abs(targetDif.ToRotation()) < MathHelper.ToRadians(120f))
                        continue;

                    float distance = targetDif.Length();
                    if (distance < range && Collision.CanHitLine(player.Center, 0, 0, target.position, target.width, target.height))
                    {
                        range = distance;
                        targetNPC = target.whoAmI;
                    }
                }
                if (targetNPC != -1)
                {
                    tank._aiming = true;

                    if (tank._frameExtraCounter == 0f)
                    {
                        // Release 3 missiles matching the animation frames
                        if (tank._frameExtra > 0 && tank._frameExtra < 4)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 rocketPos = player.Center - Vector2.UnitX * (60f - 4f * i - 6f * tank._frameExtra) * player.direction - Vector2.UnitY * (72f - 8f * i);
                                Vector2 rocketVel = (Vector2.UnitX * player.direction).RotatedBy(-MathHelper.PiOver4 * player.direction) * 10f;
                                int rocketDamage = (int)player.GetBestClassDamage().ApplyTo(800);
                                float rocketKB = 1f;
                                Projectile.NewProjectile(new EntitySource_Parent(player), rocketPos, rocketVel, ModContent.ProjectileType<ExoTankMissile>(), rocketDamage, rocketKB, player.whoAmI);
                            }
                        }

                        // Fires a bullet on every frame switch
                        Vector2 bulletPos = player.Center + Main.rand.NextVector2Circular(3f, 3f) + Vector2.UnitX * 80f * player.direction - Vector2.UnitY * 8f;
                        Vector2 bulletVel = (Main.npc[targetNPC].Center - bulletPos).SafeNormalize(Vector2.UnitX * player.direction) * Main.rand.NextFloat(10f, 12f);
                        int bulletDamage = (int)player.GetBestClassDamage().ApplyTo(400);
                        float bulletKB = 1f;
                        Projectile laser = Projectile.NewProjectileDirect(new EntitySource_Parent(player), bulletPos, bulletVel, ModContent.ProjectileType<AtlasMunitionsLaser>(), bulletDamage, bulletKB, player.whoAmI);
                        if (laser.whoAmI.WithinBounds(Main.maxProjectiles))
                            laser.DamageType = DamageClass.Generic;
                    }
                }
                else
                    tank._aiming = false;
            }
        }

        public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
        {
            // Locked to 2 states: moving and not moving
            if (state != 0)
                state = 1;

            // Advances weapon frames while attacking OR while not attacking but mid-animation
            Mount tank = mountedPlayer.mount;
            if (tank._aiming || tank._frameExtra > 0)
                tank._frameExtraCounter++;

            if (tank._frameExtraCounter >= 5f)
            {
                tank._frameExtraCounter = 0f;
                tank._frameExtra++;
                if (tank._frameExtra >= 8)
                    tank._frameExtra = 0;
            }
            return true;
        }

        public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
        {
            // Modify frames for backTextureExtra (weapons) and its glow
            if (drawType == 1)
                frame = texture.Frame(1, 8, 0, drawPlayer.mount._frameExtra);

            return true;
        }
    }
}
