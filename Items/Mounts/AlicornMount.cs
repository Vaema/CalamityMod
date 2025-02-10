using CalamityMod.Buffs.Mounts;
using CalamityMod.CalPlayer;
using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class AlicornMount : ModMount
    {
        public override void SetStaticDefaults()
        {
            MountData.spawnDust = 234;
            MountData.spawnDustNoGravity = true;
            MountData.buff = ModContent.BuffType<AlicornBuff>();

            // Horizontal movement
            MountData.runSpeed = 5.6f;
            MountData.dashSpeed = 17.6f;
            MountData.swimSpeed = 4f;
            MountData.acceleration = 0.4f;

            // Vertical movement
            MountData.fallDamage = 0f;
            MountData.fatigueMax = int.MaxValue;
            MountData.flightTimeMax = int.MaxValue;
            MountData.jumpSpeed = 9.21f;

            // Frames and offsets
            MountData.totalFrames = 15;
            MountData.heightBoost = 34;
            int baseYOffset = 30;
            int[] array = new int[MountData.totalFrames];
            for (int l = 0; l < array.Length; l++)
                array[l] = baseYOffset;

            array[1] = array[3] = array[5] = array[7] = array[12] = baseYOffset - 2;
            MountData.playerYOffsets = array;
            MountData.playerHeadOffset = 36;
            MountData.bodyFrame = 3;
            MountData.xOffset = -4;
            MountData.yOffset = 6;
            MountData.standingFrameCount = 1;
            MountData.standingFrameDelay = 12;
            MountData.standingFrameStart = 0;
            MountData.runningFrameCount = 8;
            MountData.runningFrameDelay = 42;
            MountData.runningFrameStart = 1;
            MountData.flyingFrameCount = 6;
            MountData.flyingFrameDelay = 4;
            MountData.flyingFrameStart = 9;
            MountData.inAirFrameCount = 1;
            MountData.inAirFrameDelay = 12;
            MountData.inAirFrameStart = 10;
            MountData.idleFrameCount = 1;
            MountData.idleFrameDelay = 12;
            MountData.idleFrameStart = 5;
            MountData.idleFrameLoop = true;
            MountData.swimFrameCount = MountData.inAirFrameCount;
            MountData.swimFrameDelay = MountData.inAirFrameDelay;
            MountData.swimFrameStart = MountData.inAirFrameStart;
            if (!Main.dedServ)
            {
                MountData.frontTextureExtra = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/AlicornMountExtra");
                MountData.textureWidth = MountData.backTexture.Width();
                MountData.textureHeight = MountData.backTexture.Height();
            }
        }

        public override void SetMount(Player player, ref bool skipDust)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.type == ModContent.NPCType<FAP>())
                {
                    npc.active = false;
                    npc.netUpdate = true;
                    break;
                }
            }
        }

        public override void Dismount(Player player, ref bool skipDust)
        {
            bool anyPlayerOnFabMount = false;
            foreach (Player player2 in Main.ActivePlayers)
            {
                // The player that is dismounting is technically not on the mount anymore.
                if (player2.Calamity().fab && player2.whoAmI != player.whoAmI)
                {
                    anyPlayerOnFabMount = true;
                    break;
                }
            }

            // Spawn Cirrus if no other players are on the Alicorn mount.
            if (!anyPlayerOnFabMount)
            {
                if (!NPC.AnyNPCs(ModContent.NPCType<FAP>()))
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.NewNPC(NPC.GetSource_TownSpawn(), (int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<FAP>());
                }
            }
        }

        public override void UpdateEffects(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.fabsolVodka)
                player.GetDamage<GenericDamageClass>() += 0.1f;

            if (player.velocity.Length() > 9f)
            {
                int rand = Main.rand.Next(2);
                bool momo = false;
                if (rand == 1)
                {
                    momo = true;
                }
                Color meme;
                if (momo)
                {
                    meme = new Color(255, 68, 242);
                }
                else
                {
                    meme = new Color(25, 105, 255);
                }
                Rectangle rect = player.getRect();
                int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.BoneTorch, 0, 0, 0, meme);
                Main.dust[dust].noGravity = true;
            }

            if (player.velocity.Y != 0f)
            {
                if (player.mount.PlayerOffset == 28)
                {
                    if (!player.flapSound)
                        SoundEngine.PlaySound(SoundID.Item32, player.Center);
                    player.flapSound = true;
                }
                else
                    player.flapSound = false;
            }
        }
    }
}
