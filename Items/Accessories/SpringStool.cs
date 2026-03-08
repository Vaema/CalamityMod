using System;
using System.Collections.Generic;
using CalamityMod.Cooldowns;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class SpringStool : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.SpringStoolJumpHotKey);
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((JumpCooldown).FramesToSeconds(), (CritRateBoostAboveTargets));

        public static int JumpCooldown = CalamityUtils.SecondsToFrames(20);
        public static float CritRateBoostAboveTargets = 5f;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 46;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SpringStoolPlayer>().springStool = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.PortableStool).
                AddRecipeGroup("AnyCopperBar", 3).
                AddTile(TileID.Anvils).
                Register();
        }
    }


    public class SpringStoolPlayer : ModPlayer
    {
        public bool springStool = false;
        public int springStoolTimer = 0;
        public bool hasGroundedSinceJump = true;

        public override void Load()
        {
            // Hook directly into vanilla's stool drawing
            On_PlayerDrawLayers.DrawPlayer_03_PortableStool += HandleStoolStacking;
        }

        public override void PostUpdate()
        {
            bool theCollisionCheck = Collision.TileCollision(Player.position + Vector2.UnitY, Vector2.Zero, Player.width, Player.height, fallThrough: false, fall2: false).Y == 0f;

            if ((Player.velocity.Y == 0 && theCollisionCheck) || Player.grappling[0] >= 0)
                hasGroundedSinceJump = true;
        }

        public override void PostUpdateEquips()
        {
            if (CalamityKeybinds.SpringStoolJumpHotKey.JustPressed && springStool && Main.myPlayer == Player.whoAmI && !Player.HasCooldown(Stooldown.ID) && !Player.mount.Active && hasGroundedSinceJump)
            {
                springStoolTimer = 12;

                Player.AddCooldown(Stooldown.ID, (int)SpringStool.JumpCooldown, true);
                hasGroundedSinceJump = false;

                Vector2 spawnPos = Player.Bottom + new Vector2(0f, -60f);

                // Spawn stool with downward and randomly angled force
                Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, new Vector2(Main.rand.NextFloat(-1f, 1f), 1.2f), ModContent.ProjectileType<SpringStoolFX>(), 0, 0f, Player.whoAmI);
                SoundEngine.PlaySound(SoundID.Item61 with { Pitch = 0.3f, Volume = 0.7f }, Player.Center);

                if (IsVanillaStoolEquipped(Player))
                {
                    // Spawn a step stool copy with a random velocity. Only works if step stool is equipped as well as spring stool.
                    Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, new Vector2(Main.rand.NextBool() ? Main.rand.NextFloat(6f, 7f) : Main.rand.NextFloat(-6f, -7f), Main.rand.NextFloat(-8f, -10f)), ModContent.ProjectileType<StepStoolBonusFX>(), 0, 0f, Player.whoAmI);
                }
            }

            if (springStool)
            {
                bool holdingUp = Player.controlUp;
                bool standingStill = Player.velocity.Y == 0 && Math.Abs(Player.velocity.X) < 0.1f;

                if (holdingUp && standingStill && !Player.mount.Active)
                {
                    int boost = 61;
                    if (IsVanillaStoolEquipped(Player))
                        boost += 24;

                    Player.portableStoolInfo.HasAStool = true;
                    Player.portableStoolInfo.IsInUse = true;
                    Player.portableStoolInfo.HeightBoost = boost;
                    Player.portableStoolInfo.VisualYOffset = boost;
                    Player.portableStoolInfo.MapYOffset = boost;

                    // Forces the player into the stool-standing frame
                    Player.UpdatePortableStoolUsage();
                }

                else
                {
                    // Ensures the player can use the stool if they stop moving/hold up
                    Player.portableStoolInfo.HasAStool = true;
                }
            }
        }

        private void HandleStoolStacking(On_PlayerDrawLayers.orig_DrawPlayer_03_PortableStool orig, ref PlayerDrawSet drawInfo)
        {
            bool isUsingStool = drawInfo.drawPlayer.portableStoolInfo.IsInUse;
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<SpringStoolPlayer>();
            bool hasSpring = modPlayer.springStool;

            if (!isUsingStool)
            {
                orig(ref drawInfo);
                return;
            }

            if (!hasSpring)
            {
                orig(ref drawInfo);
                return;
            }

            if (IsVanillaStoolEquipped(drawInfo.drawPlayer))
            {
                orig(ref drawInfo);
                return;
            }

            return;
        }

        private bool IsVanillaStoolEquipped(Player player)
        {
            for (int k = 3; k <= 12; k++)
            {
                if (player.armor[k].type == ItemID.PortableStool)

                    return true;
            }
            return false;
        }

        public override void PreUpdateMovement()
        {
            if (springStoolTimer > 0)
            {
                springStoolTimer--;

                if (Player.whoAmI == Main.myPlayer)
                {
                    float launchPower = 20f * Utils.GetLerpValue(0, 10, springStoolTimer, true);
                    Player.velocity.Y = -launchPower * Player.gravDir;

                    // Prevent vanilla jump logic from interfering
                    Player.jump = 0;
                    Player.fallStart = (int)(Player.position.Y / 16f);
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (springStool)
            {
                // When hitting a target below you
                if (Main.LocalPlayer.Top.Y < target.Top.Y)
                {
                    // Effective +8% crit chance. Increasing crit chance additively through manual rolling is scuffed for crossmod compatability, feel free to improve it if you know a cleaner way
                    float finalCritChance = Player.GetTotalCritChance(modifiers.DamageType) + SpringStool.CritRateBoostAboveTargets;

                    if (Main.rand.NextFloat(1f, 101f) <= finalCritChance)
                        modifiers.SetCrit();

                    else
                        modifiers.DisableCrit();
                }
            }
        }

        public override void ResetEffects()
        {
            springStool = false;
            springStoolTimer = 0;
        }

        public override void UpdateDead()
        {
            springStool = false;
            springStoolTimer = 0;
            hasGroundedSinceJump = true;
        }
    }
}
