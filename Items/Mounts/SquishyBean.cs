using CalamityMod.Buffs.Mounts;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class SquishyBean : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.buff = ModContent.BuffType<SquishyBeanBuff>();

            // Horizontal movement
            MountData.runSpeed = 5f;
            MountData.dashSpeed = 8f;
            MountData.acceleration = 0.1f;

            // Vertical movement
            MountData.fallDamage = 0f;
            MountData.jumpHeight = 20;
            MountData.jumpSpeed = 15f;
            MountData.constantJump = true;

            // Frames and offsets
            MountData.totalFrames = 4;
            MountData.heightBoost = 42;
            int[] array = new int[MountData.totalFrames];
            for (int l = 0; l < array.Length; l++)
            {
                switch (l)
                {
                    case 0:
                        array[l] = 46;
                        break;
                    case 1:
                        array[l] = 48;
                        break;
                    case 2:
                        array[l] = 50;
                        break;
                    case 3:
                        array[l] = 50;
                        break;
                }
            }
            MountData.playerYOffsets = array;
            MountData.playerHeadOffset = 30;
            MountData.bodyFrame = 3;
            MountData.xOffset = 0;
            MountData.yOffset = 11;
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            MountData.runningFrameCount = 4;
            MountData.runningFrameDelay = 24;
            MountData.runningFrameStart = 0;
            MountData.flyingFrameCount = 0;
            MountData.flyingFrameDelay = 0;
            MountData.flyingFrameStart = 0;
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 0;
            MountData.idleFrameCount = 4;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 0;
            MountData.idleFrameLoop = true;
            MountData.flyingFrameCount = 4;
            MountData.flyingFrameDelay = 12;
            MountData.flyingFrameStart = 0;
            MountData.swimFrameCount = MountData.inAirFrameCount;
            MountData.swimFrameDelay = MountData.inAirFrameDelay;
            MountData.swimFrameStart = MountData.inAirFrameStart;
            if (!Main.dedServ)
            {
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }

        public override void UpdateEffects(Player player)
        {
            if (player.velocity.Y > 0f || player.controlDown)
            {
                player.gravity = 1f;
                player.maxFallSpeed = 20f;
            }
        }
    }
}

