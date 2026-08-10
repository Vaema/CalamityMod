using System;
using System.Collections.Generic;
using CalamityMod.Buffs.Mounts;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts;

public class ExoTank : ModMount
{
    public static int DashDamage = 6000;

    public static int MaxHoverTime = 600;

    public static int MissileDamage = 1000;
    public static int MissileAttackRate = 4;
    public static int MissileLauncherFrameCount = 8;
    public static int MissileReuseDelay = 28; // Total use time = AttackRate * FrameCount + ReuseDelay (AttackRate being the "use animation" for the clockwork weapon)
    public static readonly SoundStyle MissileLaunchSound = new("CalamityMod/Sounds/Custom/ExoMechs/ArtemisApolloDash") { Volume = 0.5f };

    public static int MinigunDamage = 400;
    public static int MinigunAttackRate = 4;
    public static int MinigunFrameCount = 4;

    public class ExoTankData
    {
        internal bool Hovering;
        internal int HoverTime;
        internal int MinigunFrame;
        internal int MinigunFrameCounter;
        internal float MinigunRotation;

        public ExoTankData()
        {
            Hovering = false;
            HoverTime = MaxHoverTime;
            MinigunFrame = 0;
            MinigunFrameCounter = 0;
            MinigunRotation = 0f;
        }
    }

    public override void SetStaticDefaults()
    {
        MountData.spawnDust = DustID.Platinum;
        MountData.spawnDustNoGravity = true;
        MountData.buff = ModContent.BuffType<ExoTankBuff>();

        // Horizontal movement
        MountData.runSpeed = 10f;
        MountData.dashSpeed = 22.5f;
        MountData.swimSpeed = 10f;
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
        MountData.playerHeadOffset = 80;
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
        if (!Main.dedServ)
        {
            // back (back layer) -> backExtra (gun) -> front (launcher) -> frontExtra (front layer)
            MountData.backTextureExtra = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTank_BackGun");
            MountData.backTextureExtraGlow = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTank_BackGunGlow");
            MountData.frontTextureGlow = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTank_FrontGlow");
            MountData.frontTextureExtra = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTank_FrontLayer");
            MountData.frontTextureExtraGlow = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/ExoTank_FrontLayerGlow");
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    public override void SetMount(Player player, ref bool skipDust) => player.mount._mountSpecificData = new ExoTankData();

    public override void Dismount(Player player, ref bool skipDust)
    {
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ExoTankHoverThrust>())
            {
                proj.Kill();
                break;
            }
        }
    }

    public override void UpdateEffects(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            Mount tank = player.mount;
            var data = (ExoTankData)player.mount._mountSpecificData;
            ref float minigunRotation = ref data.MinigunRotation;

            // Find the closest NPC targetable by the tank
            float range = 960f; // 60 tiles
            int targetNPC = -1;
            bool canFireLasers = true;
            Vector2 gunPosition = player.Center + Vector2.UnitX * 52f * player.direction - Vector2.UnitY * 10f;
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!target.CanBeChasedBy(tank))
                    continue;

                Vector2 targetDif = target.Center - gunPosition;
                float angle = MathF.Abs(targetDif.ToRotation());

                // The tank has a base sight line of 120 degrees, equal to Toy Tank
                // However, the laser cannon can only pivot up to 36 degrees and any further will not fire out lasers
                if (player.direction == 1)
                {
                    if (angle > MathHelper.ToRadians(60f))
                        continue;

                    if (canFireLasers && angle > MathHelper.ToRadians(18f))
                        continue;
                }
                else
                {
                    if (angle < MathHelper.ToRadians(120f))
                        continue;

                    if (canFireLasers && angle < MathHelper.ToRadians(162f))
                        continue;
                }

                float distance = targetDif.Length();
                if (distance < range && Collision.CanHitLine(gunPosition, 0, 0, target.position, target.width, target.height))
                {
                    range = distance;
                    targetNPC = target.whoAmI;
                    canFireLasers = (player.direction == 1 ? angle <= MathHelper.ToRadians(18f) : angle >= MathHelper.ToRadians(162f));
                }
            }

            var source = new EntitySource_Mount(player, Type);

            // Release 3 missiles matching the animation frames
            if ((int)tank._frameExtraCounter % MissileAttackRate == 0)
            {
                if (tank._frameExtra > 0 && tank._frameExtra < 4)
                {
                    if (tank._frameExtra == 1)
                        SoundEngine.PlaySound(MissileLaunchSound, player.Center);

                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 rocketPos = player.Center - Vector2.UnitX * (60f - 4f * i - 6f * tank._frameExtra) * player.direction - Vector2.UnitY * (72f - 8f * i);
                        Vector2 rocketVel = (Vector2.UnitX * player.direction).RotatedBy(-MathHelper.PiOver4 * player.direction) * 10f;
                        int rocketDamage = (int)player.GetBestClassDamage().ApplyTo(MissileDamage);
                        float rocketKB = 1f;
                        Projectile.NewProjectile(source, rocketPos, rocketVel, ModContent.ProjectileType<ExoTankMissile>(), rocketDamage, rocketKB, player.whoAmI);
                    }
                }
            }

            if (targetNPC != -1)
            {
                tank._aiming = true;
                if (!canFireLasers)
                {
                    minigunRotation = 0f;
                    return;
                }

                float idealRotation = (Main.npc[targetNPC].Center - gunPosition).SafeNormalize(Vector2.UnitX * player.direction).ToRotation();
                /*
                // Cap the rotation 
                // EDIT: This looks jarring if you can't fire lasers and the lasers are pointless if they are shot anyway. Re-add if we wanted it to shoot something again
                if (!canFireLasers)
                    idealRotation = (player.direction == 1 ? MathHelper.ToRadians(18f) : MathHelper.ToRadians(162f)) * MathF.Sign(idealRotation);
                */

                minigunRotation = idealRotation + (player.direction == 1 ? 0f : MathHelper.Pi);

                // Fires a bullet on every frame switch
                if (data.MinigunFrameCounter % MinigunAttackRate == 0)
                {
                    SoundEngine.PlaySound(CommonCalamitySounds.ExoLaserShootSound with { Volume = 0.32f }, player.Center);

                    Vector2 bulletPos = player.Center + Vector2.UnitX * 28f * player.direction - Vector2.UnitY * 10f + (idealRotation * (player.direction == 1 ? 1.33f : 1f)).ToRotationVector2() * 38f;
                    Vector2 bulletVel = (Main.npc[targetNPC].Center - bulletPos).SafeNormalize(Vector2.UnitX * player.direction) * Main.rand.NextFloat(15f, 16f);
                    int bulletDamage = (int)player.GetBestClassDamage().ApplyTo(MinigunDamage);
                    float bulletKB = 1f;
                    Projectile.NewProjectile(source, bulletPos, bulletVel, ModContent.ProjectileType<ExoTankLaser>(), bulletDamage, bulletKB, player.whoAmI);

                    Vector2 flashPos = bulletPos + bulletVel.SafeNormalize(Vector2.Zero) * 36f * (player.direction == 1 ? 1f : 1.33f);
                    for (int i = 0; i < 2; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(flashPos, DustID.FireworksRGB);
                        dust.noGravity = true;
                        dust.noLight = true;
                        dust.velocity = bulletVel.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(0.5f, 1f);
                        dust.color = Color.Lerp(Color.OrangeRed, Color.Red, Main.rand.NextFloat(0f, 0.8f));
                        dust.scale = Main.rand.NextFloat(0.5f, 0.8f);
                    }
                    // Muzzle flash
                    // This would be attached to the gun but the frame rate is too fast for only one to exist at a time
                    Projectile.NewProjectile(source, flashPos, bulletVel, ModContent.ProjectileType<ExoTankMuzzleFlash>(), 0, 0f, player.whoAmI);
                }
            }
            else
            {
                tank._aiming = false;
                minigunRotation = 0f;
            }
        }
    }

    public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
    {
        Mount tank = mountedPlayer.mount;
        var data = (ExoTankData)tank._mountSpecificData;

        // Hover effect
        // If grounded, your hover time is reset to max
        ref int hoverTime = ref data.HoverTime;
        ref bool hovering = ref data.Hovering;
        if (state <= 1)
        {
            hoverTime = MaxHoverTime;
            hovering = false;
        }
        else
        {
            if (mountedPlayer.controlJump && mountedPlayer.TryingToHoverDown && hoverTime > 0)
            {
                var source = new EntitySource_Mount(mountedPlayer, Type);
                if (mountedPlayer.ownedProjectileCounts[ModContent.ProjectileType<ExoTankHoverThrust>()] < 1)
                    Projectile.NewProjectile(source, mountedPlayer.Bottom, Vector2.Zero, ModContent.ProjectileType<ExoTankHoverThrust>(), 0, 0f, mountedPlayer.whoAmI);

                hoverTime--;
                hovering = true;
                mountedPlayer.velocity.Y -= 0.4f * mountedPlayer.gravDir;
                mountedPlayer.velocity.Y *= 0.9f;
                if (MathF.Abs(mountedPlayer.velocity.Y) < 0.05f)
                    mountedPlayer.velocity.Y = 0.00001f;
            }
            else
                hovering = false;
        }

        // Ground dust as you travel (State 1 indicates running on the ground)
        if (state == 1 && Math.Abs(velocity.X) > mountedPlayer.mount.RunSpeed)
        {
            for (int i = 0; i < 2; i++)
            {
                Dust ground = Dust.NewDustDirect(mountedPlayer.BottomLeft - Vector2.UnitX * 40f, mountedPlayer.width + 80, 6, DustID.Platinum);
                ground.velocity = new Vector2(velocity.X * 0.15f, Main.rand.NextFloat(-2f, 0f));
                ground.noLight = true;
                ground.scale = Main.rand.NextFloat(0.2f, 0.8f);
                ground.fadeIn = Main.rand.NextFloat(1f, 1.5f);
                ground.shader = GameShaders.Armor.GetSecondaryShader(mountedPlayer.cMount, mountedPlayer);
            }
            if (mountedPlayer.cMount == 0)
            {
                float direction = Math.Sign(velocity.X);
                mountedPlayer.position += new Vector2(direction * 24f, 0f);
                mountedPlayer.FloorVisuals(true);
                mountedPlayer.position -= new Vector2(direction * 24f, 0f);
            }
        }

        // Animation frames locked to 2 states: moving and not moving
        if (state != 0)
            state = 1;

        // Advances weapon frames while attacking OR while not attacking but mid-animation
        if (tank._aiming || (tank._frameExtraCounter >= MissileAttackRate))
            tank._frameExtraCounter++;
        else if (tank._frameExtraCounter > 0f)
            tank._frameExtraCounter = 0f;

        int totalUseTime = MissileReuseDelay + MissileAttackRate * MissileLauncherFrameCount;
        if (tank._frameExtraCounter >= totalUseTime)
        {
            tank._frameExtraCounter = 0f;
            tank._frameExtra = 0;
        }
        tank._frameExtra = (int)Utils.Remap(tank._frameExtraCounter, MissileReuseDelay, totalUseTime, 0f, MissileLauncherFrameCount, true);

        // Minigun has a separate custom frame counter
        ref int minigunFrame = ref data.MinigunFrame;
        ref int minigunFrameCounter = ref data.MinigunFrameCounter;
        if (tank._aiming || minigunFrameCounter > 0)
            minigunFrameCounter++;

        if (minigunFrameCounter >= MinigunAttackRate * MinigunFrameCount)
        {
            minigunFrameCounter = 0;
            minigunFrame = 0;
        }
        minigunFrame = minigunFrameCounter / MinigunAttackRate;
        return true;
    }

    public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
    {
        // Modify everything for backTextureExtra (minigun) and its glow
        if (drawType == 1)
        {
            var data = (ExoTankData)drawPlayer.mount._mountSpecificData;
            frame = texture.Frame(1, MinigunFrameCount, 0, data.MinigunFrame);
            drawPosition += new Vector2(drawPlayer.direction == 1 ? 15f : -15f, 7f);
            spriteEffects = drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            rotation = data.MinigunRotation;
            drawOrigin = new Vector2(drawPlayer.direction == 1 ? 0f : 80f, 12f);
        }

        // Frames for frontTexture (missile launcher) and its glow
        if (drawType == 2)
            frame = texture.Frame(1, MissileLauncherFrameCount, 0, drawPlayer.mount._frameExtra);

        return true;
    }
}
