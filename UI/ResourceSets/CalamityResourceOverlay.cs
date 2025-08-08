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
        public string baseFolder = "CalamityMod/UI/ResourceSets/";

        // Determines which health UI to draw based on player upgrades.
        public string LifeTexturePath()
        {
            string folder = $"{baseFolder}HP";
            CalamityPlayer modPlayer = Main.LocalPlayer.Calamity();
            if (modPlayer.chaliceOfTheBloodGod) // dozezoze - Chalice gets it's own heart color to make bleed indicator contrast consistent, and also because it looks cool
                return folder + "Chalice";
            if (modPlayer.sStrawberry)
                return folder + "SacredStrawberry";
            if (modPlayer.tCloudberry)
                return folder + "TaintedCloudberry";
            if (modPlayer.mFruit)
                return folder + "MiracleFruit";
            if (modPlayer.sTangerine)
                return folder + "SanguineTangerine";
            return string.Empty;
        }

        // Determines which mana UI to draw based on player upgrades.
        public string ManaTexturePath()
        {
            string folder = $"{baseFolder}MP";
            CalamityPlayer modPlayer = Main.LocalPlayer.Calamity();
            if (Main.LocalPlayer.statMana < 0)
                return folder + "ManaBurn";
            if (modPlayer.pHeart)
                return folder + "PhantomHeart";
            if (modPlayer.eCore)
                return folder + "EtherealCore";
            if (modPlayer.cShard)
                return folder + "CometShard";
            return string.Empty;
        }

        public override void PostDrawResource(ResourceOverlayDrawContext context)
        {
            Asset<Texture2D> asset = context.texture;
            // Vanilla texture paths
            string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
            string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

            if (ManaTexturePath() != string.Empty)
            {
                // Draw stars for Classic and Fancy
                if (asset == TextureAssets.Mana || CompareAssets(asset, fancyFolder + "Star_Fill"))
                {
                    context.texture = ModContent.Request<Texture2D>(ManaTexturePath() + "Star");
                    context.Draw();
                }
                // Draw mana bars
                else if (CompareAssets(asset, barsFolder + "MP_Fill"))
                {
                    context.texture = ModContent.Request<Texture2D>(ManaTexturePath() + "Bar");
                    context.Draw();
                }
            }

            if (LifeTexturePath() == string.Empty)
                return;

            // Draw hearts for Classic and Fancy
            if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2 || CompareAssets(asset, fancyFolder + "Heart_Fill") || CompareAssets(asset, fancyFolder + "Heart_Fill_B"))
            {
                context.texture = ModContent.Request<Texture2D>(LifeTexturePath() + "Heart");
                context.Draw();
            }
            // Draw health bars
            else if (CompareAssets(asset, barsFolder + "HP_Fill") || CompareAssets(asset, barsFolder + "HP_Fill_Honey"))
            {
                context.texture = ModContent.Request<Texture2D>(LifeTexturePath() + "Bar");
                context.Draw();
            }
        }

        // dozezoze - this method is where Chalice's bleed overlay is drawn. This can almost certainly be optimized, but it shouldn't cause problems.
        public override void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText)
        {
            var Player = Main.LocalPlayer;
            var CalPlayer = Player.Calamity();

            if (drawingLife)
            {
            if (!CalPlayer.chaliceOfTheBloodGod) return;
            var bleed = CalPlayer.chaliceBleedoutBuffer;
            var hearts = snapshot.AmountOfLifeHearts;
                var height = 0;
                int drawType = -1; //0 - bars 1- regular heart 2 - fancy heart
                Vector2 position = new Vector2(Main.screenWidth - 60, 28);

                // This sets which bleed overlay draw style to use, as well as the position needed for the selected stat display
                switch (displaySet.NameKey)
                {
                    case "HorizontalBarsWithText":
                        height = 12;
                        drawType = 0;
                        break;
                    case "HorizontalBarsWithFullText":
                        height = 12;
                        position.Y -= 2;
                        drawType = 0;
                        break;
                    case "HorizontalBars":
                        height = 12;
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
                    ModContent.Request<Texture2D>("CalamityMod/UI/ResourceSets/HPChaliceBleedBar").Value.GetData(barTextureData);
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
                    Texture2D heartTexture = ModContent.Request<Texture2D>("CalamityMod/UI/ResourceSets/HPChaliceBleedHeart").Value;
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
                    Texture2D heartTexture = ModContent.Request<Texture2D>("CalamityMod/UI/ResourceSets/HPChaliceBleedHeart").Value;
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
            var mana = -Player.statMana;
            var stars = snapshot.AmountOfManaStars;
                var height = 0;
                int drawType = -1; //0 - bars 1- regular heart 2 - fancy heart
                Vector2 position = new Vector2(Main.screenWidth - 70, 52);

                // This sets which bleed overlay draw style to use, as well as the position needed for the selected stat display
                switch (displaySet.NameKey)
                {
                    case "HorizontalBarsWithText":
                        height = 12;
                        drawType = 0;
                        break;
                    case "HorizontalBarsWithFullText":
                        height = 12;
                        position.Y -= 2;
                        drawType = 0;
                        break;
                    case "HorizontalBars":
                        height = 12;
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
                    GraphicsDevice _graphicsDevice = Main.graphics.GraphicsDevice;
                    var barOverlay = new Texture2D(_graphicsDevice, width, 12);
                    Color[] barTextureData = new Color[12*12];
                    ModContent.Request<Texture2D>(ManaTexturePath() + "Bar").Value.GetData(barTextureData);
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
                            bleedCol = barTextureData[((i % 12)+(i/width)*12)];
                        }
                        textureData[i] = bleedCol;

                    }

                    barOverlay.SetData(textureData);
                    Main.spriteBatch.Draw(barOverlay, position, null, Color.White, 0, new Vector2(width, 0), 1, SpriteEffects.None, 1);
                }
                else if (drawType == 1) //default heart
                {
                    Texture2D heartTexture = ModContent.Request<Texture2D>(ManaTexturePath() + "Star").Value;
                    for (int i = 0; i < stars; i++)
                    {
                        Vector2 PosOffset = new Vector2(0, 28*i);
                        var opacity = Math.Clamp(-(i * snapshot.ManaPerSegment - snapshot.Mana) / snapshot.ManaPerSegment, -100, 100);
                        if ((i) * snapshot.ManaPerSegment > -snapshot.Mana) opacity = 0f;
                        opacity = Math.Clamp(-((i * snapshot.ManaPerSegment - (float)mana) / snapshot.ManaPerSegment), 0, 1);
                        Main.spriteBatch.Draw(heartTexture, position + PosOffset, null, Color.White, 0, heartTexture.Size() / 2, 1 * opacity, SpriteEffects.None, 1);
                    }
                }
                else if (drawType == 2) //fancy heart. this is the same as default but with different distances between hearts
                {
                    Texture2D heartTexture = ModContent.Request<Texture2D>(ManaTexturePath() + "Star").Value;
                    for (int i = 0; i < stars; i++)
                    {
                        Vector2 PosOffset = new Vector2(0, 22*i);
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
