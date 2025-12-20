using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace CalamityMod.UI.ResourceSets
{
    public class CalamityResourceOverlay : ModResourceOverlay
    {
        // Most of this is taken from ExampleMod. See that for additional explanations.
        private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();

        // Determines which health UI to draw based on player upgrades.
        public static CalamityUIResourceSet GetLifeTextureSet()
        {
            CalamityPlayer modPlayer = Main.LocalPlayer.Calamity();
            if (modPlayer.chaliceOfTheBloodGod) // dozezoze - Chalice gets it's own heart color to make bleed indicator contrast consistent, and also because it looks cool
                return CalamityUIResourceSets.HPChalice;
            if (modPlayer.sStrawberry)
                return CalamityUIResourceSets.HPSacredStrawberry;
            if (modPlayer.tCloudberry)
                return CalamityUIResourceSets.HPTaintedCloudberry;
            if (modPlayer.mFruit)
                return CalamityUIResourceSets.HPMiracleFruit;
            if (modPlayer.sTangerine)
                return CalamityUIResourceSets.HPSanguineTangerine;
            return null;
        }

        // Determines which mana UI to draw based on player upgrades.
        public static CalamityUIResourceSet GetManaTextureSet()
        {
            CalamityPlayer modPlayer = Main.LocalPlayer.Calamity();
            if (Main.LocalPlayer.statMana < 0)
                return CalamityUIResourceSets.MPManaBurn;
            if (modPlayer.pHeart)
                return CalamityUIResourceSets.MPPhantomHeart;
            if (modPlayer.eCore)
                return CalamityUIResourceSets.MPEtherealCore;
            if (modPlayer.cShard)
                return CalamityUIResourceSets.MPCometShard;
            return null;
        }

        public override void PostDrawResource(ResourceOverlayDrawContext context)
        {
            Asset<Texture2D> asset = context.texture;

            // Vanilla texture paths
            const string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
            const string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

            const string fancyHeartFill = fancyFolder + "Heart_Fill";
            const string fancyHeartFillB = fancyFolder + "Heart_Fill_B";
            const string fancyStarFill = fancyFolder + "Star_Fill";

            const string barMPFill = barsFolder + "MP_Fill";
            const string barHPFill = barsFolder + "HP_Fill";
            const string barHPFillHoney = barsFolder + "HP_Fill_Honey";

            var manaTextureSet = GetManaTextureSet();
            if (manaTextureSet != null)
            {
                // Draw stars for Classic and Fancy
                if (asset == TextureAssets.Mana || CompareAssets(asset, fancyStarFill))
                {
                    context.texture = manaTextureSet.Star;
                    context.Draw();
                }
                // Draw mana bars
                else if (CompareAssets(asset, barMPFill))
                {
                    context.texture = manaTextureSet.Bar;
                    context.Draw();
                }
            }

            var lifeTextureSet = GetLifeTextureSet();
            if (lifeTextureSet != null)
            {
                // Draw hearts for Classic and Fancy
                if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2 || CompareAssets(asset, fancyHeartFill) || CompareAssets(asset, fancyHeartFillB))
                {
                    context.texture = lifeTextureSet.Heart;
                    context.Draw();
                }
                // Draw health bars
                else if (CompareAssets(asset, barHPFill) || CompareAssets(asset, barHPFillHoney))
                {
                    context.texture = lifeTextureSet.Bar;
                    context.Draw();
                }
            }
        }

        // dozezoze - this method is where Chalice's bleed overlay is drawn. This can almost certainly be optimized, but it shouldn't cause problems.
        public override void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText)
        {
            var Player = Main.LocalPlayer;
            var CalPlayer = Player.Calamity();

            if (drawingLife)
            {
                if (!CalPlayer.chaliceOfTheBloodGod)
                    return;
                var bleed = CalPlayer.chaliceBleedoutBuffer;
                var hearts = snapshot.AmountOfLifeHearts;
                int drawType = -1; //0 - bars 1- regular heart 2 - fancy heart
                Vector2 position = new Vector2(Main.screenWidth - 60, 28);

                // This sets which bleed overlay draw style to use, as well as the position needed for the selected stat display
                switch (displaySet.NameKey)
                {
                    case "HorizontalBarsWithText":
                        drawType = 0;
                        break;
                    case "HorizontalBarsWithFullText":
                        position.Y -= 2;
                        drawType = 0;
                        break;
                    case "HorizontalBars":
                        position.Y -= 4;
                        drawType = 0;
                        break;
                    case "Default":
                        drawType = 1;
                        position = new Vector2(Main.screenWidth - 289, 43);
                        break;
                    case "New":
                        drawType = 2;
                        position = new Vector2(Main.screenWidth - 281, 30);
                        break;
                    case "NewWithText":
                        drawType = 2;
                        position = new Vector2(Main.screenWidth - 281, 36);
                        break;
                }
                if (drawType == -1)
                {
                    return; // Loaded HP style doesn't match any of the vanilla ones, so we won't draw the overlay.
                }
                if (drawType == 0) //bars
                {

                    // This works by dynamically creating a texture that it then draws over the life bar.
                    int width = 12 * snapshot.AmountOfLifeHearts;
                    GraphicsDevice _graphicsDevice = Main.graphics.GraphicsDevice;
                    var barOverlay = new Texture2D(_graphicsDevice, width, 12);
                    Color[] barTextureData = new Color[12 * 12];
                    CalamityUIResourceSets.HPChaliceBleed.Bar.Value.GetData(barTextureData);
                    var pixelsPerLife = 12f / snapshot.LifePerSegment;
                    int deadPixels = (int)Math.Floor((snapshot.LifeMax - snapshot.Life) * pixelsPerLife);
                    var bleedPixels = Math.Round(bleed * pixelsPerLife);

                    Color[] textureData = new Color[width * 12];

                    // doze 19MAR2025 - I don't know if I could optimize this any further without switching to shaders.
                    // I don't expect performance issues, but at some point might be worth adding a config to disable the indicator in case of lag or accessibility concerns 
                    for (int i = 0; i < textureData.Length; i++)
                    {
                        var i_MOD_width = i % width; //minor optimization, calculated here once instead of twice in the loop.
                        var bleedCol = Color.Transparent;
                        if ((i_MOD_width < bleedPixels + deadPixels) && !(i_MOD_width < deadPixels))
                        {
                            bleedCol = barTextureData[((i % 12) + (i / width) * 12)];
                        }
                        textureData[i] = bleedCol;

                    }

                    barOverlay.SetData(textureData);
                    Main.spriteBatch.Draw(barOverlay, position, null, Color.White, 0, new Vector2(width, 0), 1, SpriteEffects.None, 1);
                }
                else if (drawType == 1) //default heart
                {
                    Texture2D heartTexture = CalamityUIResourceSets.HPChaliceBleed.Heart.Value;
                    for (int i = 0; i < hearts; i++)
                    {
                        Vector2 PosOffset = new Vector2((i >= 10 ? i - 10 : i) * 26, Math.Min(MathF.Floor(i / 10), 1) * 26);
                        var opacity = Math.Clamp(-(i * snapshot.LifePerSegment - snapshot.Life) / snapshot.LifePerSegment, 0, 1);
                        if ((i) * snapshot.LifePerSegment > snapshot.Life) opacity = 0f;
                        opacity = Math.Clamp(-((i * snapshot.LifePerSegment - (float)bleed) / snapshot.LifePerSegment), 0, 1);
                        Main.spriteBatch.Draw(heartTexture, position + PosOffset, null, Color.White, 0, heartTexture.Size() / 2, 1 * opacity, SpriteEffects.None, 1);
                    }
                }
                else if (drawType == 2) //fancy heart. this is the same as default but with different distances between hearts
                {
                    Texture2D heartTexture = CalamityUIResourceSets.HPChaliceBleed.Heart.Value;
                    for (int i = 0; i < hearts; i++)
                    {
                        Vector2 PosOffset = new Vector2((i >= 10 ? i - 10 : i) * 24, Math.Min(MathF.Floor(i / 10), 1) * 28);
                        var opacity = Math.Clamp(-(i * snapshot.LifePerSegment - snapshot.Life) / snapshot.LifePerSegment, 0, 1);
                        if ((i) * snapshot.LifePerSegment > snapshot.Life) opacity = 0f;
                        opacity = Math.Clamp(-((i * snapshot.LifePerSegment - (float)bleed) / snapshot.LifePerSegment), 0, 1);
                        Main.spriteBatch.Draw(heartTexture, position + PosOffset, null, Color.White, 0, heartTexture.Size() / 2, 1 * opacity, SpriteEffects.None, 1);
                    }
                }
            }
            // This is where Mana Burn is drawn
            else if (Player.statMana < 0)
            {
                var manaSet = GetManaTextureSet();
                var mana = -Player.statMana;
                var stars = snapshot.AmountOfManaStars;
                int drawType = -1; //0 - bars 1- regular heart 2 - fancy heart
                Vector2 position = new Vector2(Main.screenWidth - 70, 52);

                // This sets which bleed overlay draw style to use, as well as the position needed for the selected stat display
                switch (displaySet.NameKey)
                {
                    case "HorizontalBarsWithText":
                        drawType = 0;
                        break;
                    case "HorizontalBarsWithFullText":
                        position.Y -= 2;
                        drawType = 0;
                        break;
                    case "HorizontalBars":
                        position.Y -= 4;
                        drawType = 0;
                        break;
                    case "Default":
                        drawType = 1;
                        position = new Vector2(Main.screenWidth - 25, 43);
                        break;
                    case "New":
                        drawType = 2;
                        position = new Vector2(Main.screenWidth - 25, 38);
                        break;
                    case "NewWithText":
                        drawType = 2;
                        position = new Vector2(Main.screenWidth - 25, 38);
                        break;
                }
                if (drawType == -1)
                {
                    return; // Loaded HP style doesn't match any of the vanilla ones, so we won't draw the overlay.
                }
                if (drawType == 0) //bars
                {

                    // This works by dynamically creating a texture that it then draws over the life bar.
                    int width = 12 * stars;
                    if (width < 1)
                        width = 1;
                    GraphicsDevice _graphicsDevice = Main.graphics.GraphicsDevice;
                    var barOverlay = new Texture2D(_graphicsDevice, width, 12);
                    Color[] barTextureData = new Color[12 * 12];
                    manaSet.Bar.Value.GetData(barTextureData);
                    var pixelsPerLife = 12f / snapshot.ManaPerSegment;
                    int deadPixels = (int)Math.Floor((snapshot.ManaMax + snapshot.Mana) * pixelsPerLife);
                    var bleedPixels = Math.Round(mana * pixelsPerLife);

                    Color[] textureData = new Color[width * 12];

                    // doze 19MAR2025 - I don't know if I could optimize this any further without switching to shaders.
                    // I don't expect performance issues, but at some point might be worth adding a config to disable the indicator in case of lag or accessibility concerns 
                    for (int i = 0; i < textureData.Length; i++)
                    {
                        var i_MOD_width = i % width; //minor optimization, calculated here once instead of twice in the loop.
                        var bleedCol = Color.Transparent;
                        if ((i_MOD_width < bleedPixels + deadPixels) && !(i_MOD_width < deadPixels))
                        {
                            bleedCol = barTextureData[((i % 12) + (i / width) * 12)];
                        }
                        textureData[i] = bleedCol;

                    }

                    barOverlay.SetData(textureData);
                    Main.spriteBatch.Draw(barOverlay, position, null, Color.White, 0, new Vector2(width, 0), 1, SpriteEffects.None, 1);
                }
                else if (drawType == 1) //default heart
                {
                    Texture2D heartTexture = manaSet.Star.Value;
                    for (int i = 0; i < stars; i++)
                    {
                        Vector2 PosOffset = new Vector2(0, 28 * i);
                        var opacity = Math.Clamp(-(i * snapshot.ManaPerSegment - snapshot.Mana) / snapshot.ManaPerSegment, -100, 100);
                        if ((i) * snapshot.ManaPerSegment > -snapshot.Mana) opacity = 0f;
                        opacity = Math.Clamp(-((i * snapshot.ManaPerSegment - (float)mana) / snapshot.ManaPerSegment), 0, 1);
                        Main.spriteBatch.Draw(heartTexture, position + PosOffset, null, Color.White, 0, heartTexture.Size() / 2, 1 * opacity, SpriteEffects.None, 1);
                    }
                }
                else if (drawType == 2) //fancy heart. this is the same as default but with different distances between hearts
                {
                    Texture2D heartTexture = manaSet.Star.Value;
                    for (int i = 0; i < stars; i++)
                    {
                        Vector2 PosOffset = new Vector2(0, 22 * i);
                        var opacity = Math.Clamp(-(i * snapshot.ManaPerSegment - snapshot.Mana) / snapshot.ManaPerSegment, -100, 100);
                        if ((i) * snapshot.ManaPerSegment > -snapshot.Mana) opacity = 0f;
                        opacity = Math.Clamp(-((i * snapshot.ManaPerSegment - (float)mana) / snapshot.ManaPerSegment), 0, 1);
                        Main.spriteBatch.Draw(heartTexture, position + PosOffset, null, Color.White, 0, heartTexture.Size() / 2, 1 * opacity, SpriteEffects.None, 1);
                    }
                }
            }

        }

        private bool CompareAssets(Asset<Texture2D> currentAsset, string compareAssetPath)
        {
            if (!vanillaAssetCache.TryGetValue(compareAssetPath, out var asset))
                asset = vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);

            return currentAsset == asset;
        }
    }
}
