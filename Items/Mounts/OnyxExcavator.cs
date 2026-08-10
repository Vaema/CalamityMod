using System;
using System.Collections.Generic;
using CalamityMod.Buffs.Mounts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts;

// TODO:
// - don't render the player's arm
// - don't allow items to be used at all (like vanilla DCU)
// - drill animation speed is independent of mount movement speed and always maxed when picking at least one tile
// - separate animation for the drill coming out of the bottom when it digs down
public class OnyxExcavator : ModMount
{
    public static List<int> OnyxExcavatorImmuneTiles = null;
    
    public override void SetStaticDefaults()
    {
        // 05JAN2026: Ozzatron: Onyx Excavator blacklist is now public and modifiable in a semi-sane way.
        // Also removed the Abyss tiles from the blacklist because now that the drill respects tile durability, it's fine.
        OnyxExcavatorImmuneTiles =
        [
            // ModContent.TileType<AbyssGravel>(),
            // ModContent.TileType<PyreMantle>(),
            // ModContent.TileType<PyreMantleMolten>(),
            // ModContent.TileType<Voidstone>(),
            TileID.DemonAltar,
            TileID.ElderCrystalStand,
        ];

        // Dust that spawns upon mounting or unmounting
        MountData.spawnDust = DustID.Orichalcum;
        MountData.spawnDustNoGravity = true;
        MountData.buff = ModContent.BuffType<OnyxExcavatorBuff>();

        // Horizontal movement
        MountData.runSpeed = 4.5f;
        MountData.swimSpeed = 0.5f;
        MountData.acceleration = 0.1f;

        // Vertical movement
        // 22JAN2026: Ozzatron: Onyx Excavator Drill does not make you immune to fall damage and blocks double jumps
        MountData.jumpHeight = 5;
        MountData.jumpSpeed = 3f;
        MountData.blockExtraJumps = true;

        // Frames and offsets
        MountData.totalFrames = 6;
        MountData.heightBoost = 10;
        int[] array = new int[MountData.totalFrames];
        for (int l = 0; l < array.Length; l++)
            array[l] = 6;

        // 22JAN2026: Ozzatron: removed copy pasted code that made the player's head bob like Unicorn
        MountData.playerYOffsets = array;
        MountData.playerHeadOffset = 10;
        MountData.bodyFrame = 3;
        MountData.xOffset = 10;
        MountData.yOffset = -1; //done
        
        MountData.standingFrameCount = 1;
        MountData.standingFrameDelay = 12;
        MountData.standingFrameStart = 0;
        MountData.runningFrameCount = 6;
        // 22JAN2026: Ozzatron: vastly increased animation speed so the drill and wheels spin at appropriate speeds
        MountData.runningFrameDelay = 12; //36
        MountData.runningFrameStart = MountData.standingFrameStart;
        MountData.inAirFrameCount = MountData.standingFrameCount;
        MountData.inAirFrameDelay = MountData.standingFrameDelay;
        MountData.inAirFrameStart = MountData.standingFrameStart;
        MountData.idleFrameCount = MountData.standingFrameCount;
        MountData.idleFrameDelay = MountData.standingFrameDelay;
        MountData.idleFrameStart = MountData.standingFrameStart;
        MountData.idleFrameLoop = false;
        MountData.swimFrameCount = MountData.inAirFrameCount;
        MountData.swimFrameDelay = MountData.inAirFrameDelay;
        MountData.swimFrameStart = MountData.inAirFrameStart;

        if (!Main.dedServ)
        {
            // 22JAN2026: Ozzatron: Drill itself was rendering behind the mount instead of in front of it
            MountData.frontTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/OnyxExcavatorExtra2");
            MountData.frontTextureExtra = ModContent.Request<Texture2D>("CalamityMod/Items/Mounts/OnyxExcavatorExtra");
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    public override void Unload() => OnyxExcavatorImmuneTiles = null;

    public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
    {
        bool speed = Math.Abs(velocity.X) > mountedPlayer.mount.RunSpeed / 2f;
        float direction = Math.Sign(mountedPlayer.velocity.X);
        
        Lighting.AddLight(mountedPlayer.Center, 0.5f, 0.5f, 0.4f);

        if (speed && velocity.Y == 0f)
        {
            // 22JAN2026: Ozzatron: significantly improved dust to be far less obnoxious when traveling at speed
            // looks more like it's shaking off rust as it drives around
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(mountedPlayer.BottomLeft, mountedPlayer.width, 6, DustID.RedMoss, 0f, 0f, 0, default, 1f);
                dust.velocity = new Vector2(velocity.X * 0.15f, Main.rand.NextFloat() * -2f);
                dust.noLight = true;
                dust.scale = 0.2f + Main.rand.NextFloat() * 0.8f;
                dust.fadeIn = 0.5f + Main.rand.NextFloat() * 1f;
                dust.shader = GameShaders.Armor.GetSecondaryShader(mountedPlayer.cMount, mountedPlayer);
            }

            if (mountedPlayer.cMount == 0)
            {
                mountedPlayer.position += new Vector2(direction * 24f, 0f);
                mountedPlayer.FloorVisuals(true);
                mountedPlayer.position -= new Vector2(direction * 24f, 0f);
            }
        }
        return true;
    }

    /// <summary>
    /// Attempts to wield the Onyx Excavator to drill the tile at the given position.
    /// </summary>
    /// <param name="player">The player using the Onyx Excavator.</param>
    /// <param name="targetPos">The position of the target tile.</param>
    /// <param name="pickPower">The power of the player's best pickaxe.</param>
    /// <returns>Whether the Onyx Excavator successfully mined this tile.</returns>
    private static bool OnyxExcavateTile(Player player, Point targetPos, int pickPower)
    {
        int x = targetPos.X;
        int y = targetPos.Y;
        Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);

        // Startup exclusion checks.
        if (!tile.HasTile || player.noBuilding || Main.tileContainer[tile.TileType])
            return false;
        if (OnyxExcavatorImmuneTiles.Contains(tile.TileType))
            return false;

        // This function contains all CanKillTile checks and manipulates AchievementsHelper.CurrentlyMining for us.
        // It also sends appropriate packets in multiplayer.
        player.PickTile(x, y, pickPower);
        return true;
    }

    public override void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
    {
        // Only evaluate drilling on the drill-owning client
        if (Main.myPlayer != player.whoAmI)
            return;

        // Any drilling requires holding LMB.
        bool holdingMouse = Main.mouseLeft && !player.mouseInterface && !Main.blockMouse;
        if (!holdingMouse)
            return;

        // Drilling horizontally takes precedence, but if no controls are input, the player will not dig.
        bool canDrillHorizontally = player.controlLeft || player.controlRight;
        bool canDrillDown = player.controlDown;
        if (!canDrillHorizontally && !canDrillDown)
            return;

        // Onyx Excavator Drill assumes you have an unreforged Copper Pickaxe if you have no pickaxes.
        Item bestPick = player.GetBestPick() ?? ContentSamples.ItemsByType[ItemID.CopperPickaxe];
        int pickPower = bestPick.pick;
        int digCadence = bestPick.useTime;

        // If the cadence does not match the use time of the pickaxe that is being mimicked, do nothing.
        var cgp = player.Calamity();
        if (cgp.universalFrameTimer % (ulong)digCadence != 0)
            return;

        Point[] drillTargets = null;

        // Drilling horizontally takes precedence.
        // If you want to drill straight down, you need to hold down without left or right.
        if (canDrillHorizontally)
        {
            // Determine mining direction based on the player's facing.
            // At extremely low velocities, the drill intentionally drills at closer positions.
            // This is so if you're parked against a wall it doesn't drill through the first layer from a standstill.
            float xVel = player.velocity.X;
            float absVel = Math.Abs(xVel);
            int direction = absVel < 0.1f ? player.direction : Math.Sign(xVel);
            int xTileOffset = absVel > 0.5f ? 2 : 1;

            // These are offset so they're always correct when getting floored by integer division
            int playerLeadingEdgeX = direction == -1 ? player.Hitbox.Left + 2 : player.Hitbox.Right - 2;
            int bottomTileCenterY = player.Hitbox.Bottom - 8;
            int drillLeadingEdgeX = playerLeadingEdgeX + direction * 16 * xTileOffset;

            // Forms the following pattern:
            // |X
            // |XX
            // |XX
            // |X
            Vector2 drillOrigin     = new(drillLeadingEdgeX, bottomTileCenterY);
            Vector2 drillOneUp      = drillOrigin + new Vector2(0f, -16f);
            Vector2 drillTwoUp      = drillOrigin + new Vector2(0f, -32f);
            Vector2 drillThreeUp    = drillOrigin + new Vector2(0f, -48f);
            Vector2 drillFrontLower = drillOrigin + new Vector2(16f * direction, -16f);
            Vector2 drillFrontUpper = drillOrigin + new Vector2(16f * direction, -32f);

            drillTargets =
            [
                drillOrigin.ToSafeTileCoordinates(),
                drillOneUp.ToSafeTileCoordinates(),
                drillTwoUp.ToSafeTileCoordinates(),
                drillThreeUp.ToSafeTileCoordinates(),
                drillFrontLower.ToSafeTileCoordinates(),
                drillFrontUpper.ToSafeTileCoordinates(),
            ];
        }

        else if(canDrillDown)
        {
            // Nothing needs to be considered. Simply dig straight down. Players are 2 tiles wide so this will always work.
            int playerLeftEdgeX = player.Hitbox.Left + 2;
            int playerRightEdgeX = player.Hitbox.Right - 2;
            int drillLeadingEdgeY = player.Hitbox.Bottom + 8;

            // Two horizontal lines of four tiles each, covering the bottom part of the mount and the floor beneath it.
            Vector2 drillLeftNear  = new(playerLeftEdgeX, drillLeadingEdgeY);
            Vector2 drillRightNear = new(playerRightEdgeX, drillLeadingEdgeY);
            Vector2 drillLeftFar   = drillLeftNear + new Vector2(-16f, 0f);
            Vector2 drillRightFar  = drillRightNear + new Vector2(16f, 0f);
            Vector2 mountLeftNear  = drillLeftNear + new Vector2(0f, -16f);
            Vector2 mountRightNear = drillRightNear + new Vector2(0f, -16f);
            Vector2 mountLeftFar   = drillLeftFar + new Vector2(0f, -16f);
            Vector2 mountRightFar  = drillRightFar + new Vector2(0f, -16f);

            drillTargets =
            [
                mountLeftNear.ToSafeTileCoordinates(),
                mountRightNear.ToSafeTileCoordinates(),
                mountLeftFar.ToSafeTileCoordinates(),
                mountRightFar.ToSafeTileCoordinates(),
                drillLeftNear.ToSafeTileCoordinates(),
                drillRightNear.ToSafeTileCoordinates(),
                drillLeftFar.ToSafeTileCoordinates(),
                drillRightFar.ToSafeTileCoordinates(),
            ];
        }

        if (drillTargets is null)
            return;

        // Execute drilling on all intended target tiles.
        foreach (Point p in drillTargets)
            OnyxExcavateTile(player, p, pickPower);
    }
}
