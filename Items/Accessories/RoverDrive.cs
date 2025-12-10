using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.DataStructures;
using CalamityMod.Items.Materials;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class RoverDrive : ModItem, ILocalizedModType, IDyeableShaderRenderer
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static Asset<Texture2D> NoiseTex;
        public static readonly SoundStyle ShieldHurtSound = new("CalamityMod/Sounds/Custom/RoverDriveHit") { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle ActivationSound = new("CalamityMod/Sounds/Custom/RoverDriveActivate") { Volume = 0.85f };
        public static readonly SoundStyle BreakSound = new("CalamityMod/Sounds/Custom/RoverDriveBreak") { Volume = 0.75f };

        public static int ShieldDurabilityMax = 20;
        public static int ShieldRechargeDelay = CalamityUtils.SecondsToFrames(10);
        public static int TotalShieldRechargeTime = CalamityUtils.SecondsToFrames(5);
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ShieldDurabilityMax, ShieldRechargeDelay.FramesToSeconds(), TotalShieldRechargeTime.FramesToSeconds());

        // Interface stuff.
        public int OwnerPlayer { get; set; }
        public float RenderDepth => IDyeableShaderRenderer.RoverDriveDepth;

        public bool ShouldDrawDyeableShader
        {
            get
            {
                if (CalamityClientConfig.Instance.EnergyShieldOpacity <= 0.0f)
                    return false;

                if (OwnerPlayer < 0 || OwnerPlayer >= Main.maxPlayers)
                    return false;

                var player = Main.player[OwnerPlayer];
                if (player is null)
                    return false;

                if (player.outOfRange || player.dead)
                    return false;

                CalamityPlayer modPlayer = player.Calamity();
                if (modPlayer.drawingParameters.RoverShieldCharge <= 0.0f)
                    return false;

                return true;
            }
        }

        // Allows item to be extractinated and specifies custom behavior instead of copying an existing item
        public override void SetStaticDefaults() => ItemID.Sets.ExtractinatorMode[Type] = Item.type;

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 30;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
            Item.MakeUsableWithChlorophyteExtractinator();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            modPlayer.roverDrive = true;
            modPlayer.roverDriveShieldVisible = !hideVisual;
        }

        // In vanity, provides a visual shield but no actual functionality
        public override void UpdateVanity(Player player) => player.Calamity().roverDriveShieldVisible = true;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string adrenTooltip = CalamityWorld.revenge ? this.GetLocalizedValue("ShieldAdren") : "";
            tooltips.FindAndReplace("[ADREN]", adrenTooltip);
        }

        // Scrappable for 3-5 wulfrum scrap or a 20% chance to get an energy core
        public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
        {
            resultType = ModContent.ItemType<WulfrumMetalScrap>();
            resultStack = Main.rand.Next(3, 6);

            if (Main.rand.NextFloat() > 0.8f)
            {
                resultStack = 1;
                resultType = ModContent.ItemType<EnergyCore>();
            }
        }

        // Complex drawcode which draws Rover Drive shields on ALL players who have it available. Supposedly.
        // This is applied as IL (On hook) which draws right before Inferno Ring.
        public void DrawDyeableShader(SpriteBatch spriteBatch)
        {
            if (OwnerPlayer < 0 || OwnerPlayer >= Main.maxPlayers)
                return;

            var player = Main.player[OwnerPlayer];
            if (player is null)
                return;

            if (player.outOfRange || player.dead)
                return;

            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.drawnAnyShieldThisFrame)
                return;

            if (modPlayer.drawingParameters.RoverShieldCharge <= 0.0f)
                return;

            // The shield very gently grows and shrinks
            float scale = 0.15f + 0.03f * (0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.5f + player.whoAmI * 0.2f));
            // If in vanity, the shield is always projected as if it's at full strength.
            float shieldStrength = modPlayer.drawingParameters.RoverShieldCharge;

            // Noise scale also grows and shrinks, although out of sync with the shield
            float noiseScale = MathHelper.Lerp(0.4f, 0.8f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.3f) * 0.5f + 0.5f);

            // Define shader parameters
            Effect shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
            shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.24f);
            shieldEffect.Parameters["blowUpPower"].SetValue(2.5f);
            shieldEffect.Parameters["blowUpSize"].SetValue(0.5f);
            shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

            // Shield opacity multiplier slightly changes, this is independent of current shield strength
            float baseShieldOpacity = 0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
            float finalShieldOpacity = baseShieldOpacity * (0.5f + 0.5f * shieldStrength);
            finalShieldOpacity *= CalamityClientConfig.Instance.EnergyShieldOpacity;

            shieldEffect.Parameters["shieldOpacity"].SetValue(finalShieldOpacity);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

            // Get the shield color.
            Color blueTint = new Color(51, 102, 255);
            Color cyanTint = new Color(71, 202, 255);
            Color wulfGreen = new Color(194, 255, 67) * 0.8f;
            Color edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.2f, blueTint, cyanTint, wulfGreen);
            Color shieldColor = blueTint;


            // Define shader parameters for shield color
            shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

            using (spriteBatch.Scope())
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BatchSetting.Additive, shieldEffect, Matrix.Identity);
                // Fetch shield noise overlay texture (this is the techy overlay fed to the shader)
                NoiseTex ??= ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/TechyNoise");
                Vector2 pos = player.MountedCenter + player.gfxOffY * Vector2.UnitY - Main.screenPosition;
                Texture2D tex = NoiseTex.Value;
                spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);
                spriteBatch.End();
            }

            modPlayer.drawnAnyShieldThisFrame = true;
        }
    }
}
