using System;
using System.Collections.Generic;
using CalamityMod.Buffs.Mounts;
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

            // Attack (lasers; rockets are determined by extra frames)
            MountData.abilityCooldown = 6;

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
