using CalamityMod.Buffs.Mounts;
using CalamityMod.Dusts;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class BrimroseChair : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.spawnDust = (int)CalamityDusts.Brimstone;
            MountData.spawnDustNoGravity = true;
            MountData.buff = ModContent.BuffType<BrimroseMount>();

            // Horizontal movement
            MountData.runSpeed = 12f;
            MountData.dashSpeed = 12f;
            MountData.acceleration = 0.2f;

            // Vertical movement
            MountData.fallDamage = 0f;
            MountData.fatigueMax = int.MaxValue;
            MountData.flightTimeMax = int.MaxValue;
            MountData.jumpSpeed = 4f;
            MountData.blockExtraJumps = true;
            MountData.usesHover = true;

            // Frames and offsets
            MountData.totalFrames = 4;
            MountData.heightBoost = 0;
            int[] array = new int[MountData.totalFrames];
            for (int l = 0; l < array.Length; l++)
                array[l] = 0;

            MountData.playerYOffsets = array;
            MountData.playerHeadOffset = 18;
            MountData.bodyFrame = 3;
            MountData.xOffset = 0;
            MountData.yOffset = 6;
            MountData.standingFrameCount = 4;
            MountData.standingFrameDelay = 4;
            MountData.standingFrameStart = 0;
            MountData.runningFrameCount = 4;
            MountData.runningFrameDelay = 16;
            MountData.runningFrameStart = 0;
            MountData.flyingFrameCount = 4;
            MountData.flyingFrameDelay = 4;
            MountData.flyingFrameStart = 0;
            MountData.inAirFrameCount = 4;
            MountData.inAirFrameDelay = 4;
            MountData.inAirFrameStart = 0;
            MountData.idleFrameCount = 4;
            MountData.idleFrameDelay = 8;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;
            MountData.swimFrameCount = 4;
            MountData.swimFrameDelay = 4;
            MountData.swimFrameStart = 0;
            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }
    }
}
