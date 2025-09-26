using System;
using CalamityMod.CalPlayer;
using CalamityMod.DataStructures;
using CalamityMod.Items.Materials;
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

namespace CalamityMod.Items.Armor.LunicCorps
{
    [AutoloadEquip(EquipType.Head)]
    public class LunicCorpsHelmet : ModItem, ILocalizedModType, IDyeableShaderRenderer
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static Asset<Texture2D> NoiseTex;
        // TODO -- Accurate shield sounds from Halo
        public static readonly SoundStyle ShieldHurtSound = new("CalamityMod/Sounds/Custom/RoverDriveHit") { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle ActivationSound = new("CalamityMod/Sounds/Custom/RoverDriveActivate") { Volume = 0.85f };
        public static readonly SoundStyle BreakSound = new("CalamityMod/Sounds/Custom/RoverDriveBreak") { Volume = 0.75f };

        public static float NonArrowDamageBoost = 0.15f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(NonArrowDamageBoost.ToPercent());

        // Set Bonus
        public static float SetBonusJumpSpeedBoost = 1f;
        public static int ShieldDurabilityMax = 50;
        // The following two values taken directly from Halo 3:
        // https://www.halopedia.org/Energy_shielding#Gameplay
        public static int ShieldRechargeDelay = CalamityUtils.SecondsToFrames(5);
        public static int TotalShieldRechargeTime = CalamityUtils.SecondsToFrames(2);

        // Interface stuff.
        public int OwnerPlayer { get; set; }
        public float RenderDepth => IDyeableShaderRenderer.HaloShieldDepth;
        public bool ShaderIsDyeable => false;

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
                if (modPlayer.drawingParameters.LunicShieldCharge <= 0.0f)
                    return false;

                return true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
            Item.defense = 14;
            Item.rare = ItemRarityID.Cyan;
            Item.Calamity().donorItem = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LunicCorpsVest>() && legs.type == ModContent.ItemType<LunicCorpsBoots>();

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.lunicCorpsSet = true;

            Color AbilityBriefColor = Color.Lerp(new Color(240, 207, 60), new Color(70, 205, 251), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f));
            // The localization is formatted strangely, but attempting to put the {0} on its own line will leave a blank space if given an empty string
            string adrenTooltip = CalamityWorld.revenge ? "\n" + this.GetLocalizedValue("ShieldAdren") : "";
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusJumpSpeedBoost.ToJumpSpeedPercent(), AbilityBriefColor.Hex3(), ShieldDurabilityMax, adrenTooltip, ShieldRechargeDelay.FramesToSeconds(), TotalShieldRechargeTime.FramesToSeconds());

            player.jumpSpeedBoost += SetBonusJumpSpeedBoost;
        }

        public override void UpdateEquip(Player player)
        {
            player.bulletDamage += NonArrowDamageBoost;
            player.specialistDamage += NonArrowDamageBoost;
            player.nightVision = true;
            player.detectCreature = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.NightVisionHelmet).
                AddIngredient<AstralBar>(6).
                AddIngredient(ItemID.ChlorophyteBar, 6).
                AddIngredient(ItemID.Glass, 20).
                AddTile(TileID.LunarCraftingStation).
                SortBeforeFirstRecipesOf(ModContent.ItemType<LunicCorpsBoots>()).
                Register();
        }

        // Complex drawcode which draws Lunic Corps shields on ALL players who have it available. Supposedly.
        // This is applied as IL (On hook) which draws right before Inferno Ring.
        public void DrawDyeableShader(SpriteBatch spriteBatch)
        {
            if (OwnerPlayer < 0 || OwnerPlayer >= Main.maxPlayers)
                return;

            var player = Main.player[OwnerPlayer];
            if (player.outOfRange || player.dead)
                return;

            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.drawnAnyShieldThisFrame)
                return;

            if (modPlayer.drawingParameters.LunicShieldCharge <= 0.0f)
                return;

            // Scale the shield is drawn at. The Lunic Corps shield sticks very close to the body to mimic Halo and occasionally pulses.
            // The "i" parameter is to make different player's shields not be perfectly synced.
            int i = player.whoAmI;
            float baseScale = 0.11f;
            float maxExtraScale = 0.013f;
            float extraScalePulseInterpolant = MathF.Pow(12f, MathF.Sin(Main.GlobalTimeWrappedHourly * 1.6f + i) - 1);
            float scale = baseScale + maxExtraScale * extraScalePulseInterpolant;
            float visualShieldStrength = modPlayer.drawingParameters.LunicShieldCharge;

            // The scale used for the noise overlay polygons also grows and shrinks
            // This is intentionally out of sync with the shield, and intentionally desynced per player
            // Don't put this anywhere less than 0.25f or higher than 1f. The higher it is, the denser / more zoomed out the noise overlay is.
            float noiseScale = MathHelper.Lerp(0.65f, 0.75f, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.87f + i));

            // Define shader parameters
            Effect shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
            shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.058f); // Scrolling speed of polygonal overlay
            shieldEffect.Parameters["blowUpPower"].SetValue(2.8f);
            shieldEffect.Parameters["blowUpSize"].SetValue(0.4f);
            shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

            // Shield opacity multiplier slightly changes, this is independent of current shield strength
            float baseShieldOpacity = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 1.95f);
            float minShieldStrengthOpacityMultiplier = 0.5f;
            float finalShieldOpacity = baseShieldOpacity * MathHelper.Lerp(minShieldStrengthOpacityMultiplier, 1f, visualShieldStrength);
            finalShieldOpacity *= CalamityClientConfig.Instance.EnergyShieldOpacity;

            shieldEffect.Parameters["shieldOpacity"].SetValue(finalShieldOpacity);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

            // Lunic Corps shields are not team specific
            Color shieldColor = new Color(201, 180, 129);
            Color primaryEdgeColor = new Color(232, 212, 175);
            Color secondaryEdgeColor = new Color(237, 205, 145);
            Color edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.2f, primaryEdgeColor, secondaryEdgeColor);

            // Define shader parameters for shield color
            shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

            Main.spriteBatch.SafeBegin(SpriteSortMode.Immediate, BatchSetting.Additive, shieldEffect, Matrix.Identity, () =>
            {
                // Fetch shield noise overlay texture (this is the polygons fed to the shader)
                NoiseTex ??= ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes2");
                Vector2 pos = player.MountedCenter + player.gfxOffY * Vector2.UnitY - Main.screenPosition;
                Texture2D tex = NoiseTex.Value;
                spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);
            });

            modPlayer.drawnAnyShieldThisFrame = true;
        }
    }
}
