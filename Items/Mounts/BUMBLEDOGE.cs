using CalamityMod.Buffs.Mounts;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts;

public class BUMBLEDOGE : ModMount
{
    public override void SetStaticDefaults()
    {
        MountData.spawnDust = 60;
        MountData.spawnDustNoGravity = true;
        MountData.buff = ModContent.BuffType<BumbledogeMount>();

        // Attack
	    	MountData.abilityCooldown = 12;

        // Horizontal movement
        MountData.runSpeed = 10f;
        MountData.dashSpeed = 14.15f;
        MountData.acceleration = 0.2f;

        // Vertical movement
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 600;
        MountData.jumpSpeed = 4f;

        // Frames and offsets
        MountData.totalFrames = 12;
        MountData.heightBoost = 32;
        int[] array = new int[MountData.totalFrames];
        for (int i = 0; i < array.Length; i++)
            array[i] = 28;

        MountData.playerYOffsets = array;
        MountData.playerHeadOffset = MountData.heightBoost;
        MountData.bodyFrame = 3;
        MountData.xOffset = 0;
        MountData.yOffset = -6;
        MountData.standingFrameDelay = 12;
        MountData.standingFrameStart = 0;
        MountData.runningFrameCount = 5;
        MountData.runningFrameDelay = 20;
        MountData.runningFrameStart = 1;
        MountData.flyingFrameCount = 4;
        MountData.flyingFrameDelay = 7;
        MountData.flyingFrameStart = 7;
        MountData.inAirFrameCount = 1;
        MountData.inAirFrameDelay = 11;
        MountData.inAirFrameStart = 8;
        MountData.idleFrameCount = 1;
        MountData.idleFrameDelay = 10;
        MountData.idleFrameStart = 0;
        MountData.idleFrameLoop = true;
        MountData.swimFrameCount = MountData.inAirFrameCount;
        MountData.swimFrameDelay = MountData.inAirFrameDelay;
        MountData.swimFrameStart = MountData.inAirFrameStart;
        MountData.dashingFrameCount = MountData.flyingFrameCount;
        MountData.dashingFrameDelay = 5;
        MountData.dashingFrameStart = MountData.flyingFrameStart;
        if (!Main.dedServ)
        {
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }
    public override void UpdateEffects(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            // Fires faster if there's target in range
            NPC Target = player.Center.MinionHoming(800f, player, false);
            if (player.mount._abilityCooldown == 0 && (Main.rand.NextBool(150) || Target != null))
            {
                player.mount._abilityCooldown = MountData.abilityCooldown;
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(20f, 4f) + Vector2.UnitX * 18f * player.direction;
                Vector2 vel = Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(24f)) * Main.rand.NextFloat(-8f, -6f);
                int damage = (int)player.GetBestClassDamage().ApplyTo(180);
                float kb = 1f;
                Projectile birb = Projectile.NewProjectileDirect(new EntitySource_Mount(player, Type), pos, vel, ModContent.ProjectileType<MiniatureFolly>(), damage, kb, player.whoAmI);
                if (birb.whoAmI.WithinBounds(Main.maxProjectiles))
                {
                    birb.DamageType = DamageClass.Generic;
                    birb.ai[2] = 1f;
                }
            }
        }
    }
}
