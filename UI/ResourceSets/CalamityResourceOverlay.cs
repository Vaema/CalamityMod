using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace CalamityMod.UI.ResourceSets;

public class CalamityResourceOverlay : ModResourceOverlay
{
    // Most of this is taken from ExampleMod. See that for additional explanations.
    private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();

    // Vanilla texture paths
    const string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
    const string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

    private Asset<Texture2D> FancyHeartFill => field ??= Main.Assets.Request<Texture2D>(fancyFolder + "Heart_Fill");
    private Asset<Texture2D> FancyHeartFillB => field ??= Main.Assets.Request<Texture2D>(fancyFolder + "Heart_Fill_B");
    private Asset<Texture2D> FancyStarFill => field ??= Main.Assets.Request<Texture2D>(fancyFolder + "Star_Fill");

    private Asset<Texture2D> BarMPFill => field ??= Main.Assets.Request<Texture2D>(barsFolder + "MP_Fill");
    private Asset<Texture2D> BarHPFill => field ??= Main.Assets.Request<Texture2D>(barsFolder + "HP_Fill");
    private Asset<Texture2D> BarHPFillHoney => field ??= Main.Assets.Request<Texture2D>(barsFolder + "HP_Fill_Honey");

    // Determines which health UI to draw based on player upgrades.
    public static CalamityUIResourceSet GetLifeTextureSet()
    {
        CalamityPlayer modPlayer = Main.LocalPlayer.Calamity();
        if (modPlayer.chaliceHeartStyle) // dozezoze - Chalice gets it's own heart color to make bleed indicator contrast consistent, and also because it looks cool
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
        if (Main.LocalPlayer.statMana < 0 && Main.LocalPlayer.Calamity().ChaosStone)
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

        var manaTextureSet = GetManaTextureSet();
        if (manaTextureSet != null)
        {
            // Draw stars for Classic and Fancy
            if (asset == TextureAssets.Mana || asset == FancyStarFill)
            {
                context.texture = manaTextureSet.Star;
                context.Draw();
            }
            // Draw mana bars
            else if (asset == BarMPFill)
            {
                context.texture = manaTextureSet.Bar;
                context.Draw();
            }
        }

        var lifeTextureSet = GetLifeTextureSet();
        if (lifeTextureSet != null)
        {
            // Draw hearts for Classic and Fancy
            if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2 || asset == FancyHeartFill || asset == FancyHeartFillB)
            {
                context.texture = lifeTextureSet.Heart;
                context.Draw();
            }
            // Draw health bars
            else if (asset == BarHPFill || asset == BarHPFillHoney)
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
            if (!CalPlayer.chaliceHeartStyle)
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
                int width = snapshot.AmountOfLifeHearts * 12;
                float pixelsPerLife = 12f / snapshot.LifePerSegment;
                int deadPixels = (int)MathF.Floor((snapshot.LifeMax - snapshot.Life) * pixelsPerLife);
                int bleedPixels = (int)MathF.Ceiling((float)bleed * pixelsPerLife);
                bleedPixels = Math.Min(bleedPixels, width - deadPixels);

                if (bleedPixels < 0)
                    return;

                int barMaxStatX = (int)position.X - width;
                int drawX = barMaxStatX + deadPixels;

                using (Main.spriteBatch.Scope())
                {
                    var bleedDrawPos = new Vector2(drawX, position.Y);
                    var bleedDrawRect = new Rectangle(deadPixels % 12, 0, bleedPixels, 12);
                    var bleedDrawOrigin = new Vector2(0, 0);
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    Main.spriteBatch.Draw(CalamityUIResourceSets.HPChaliceBleed.Bar.Value, bleedDrawPos, bleedDrawRect, Color.White, 0, bleedDrawOrigin, 1, SpriteEffects.None, 1);
                    Main.spriteBatch.End();
                }
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
        else if (Player.statMana < 0 && Player.Calamity().ChaosStone)
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
                float pixelsPerStar = 12f / snapshot.ManaPerSegment;
                int bleedPixels = (int)MathF.Ceiling((float)mana * pixelsPerStar);

                if (bleedPixels < 0)
                    return;

                using (Main.spriteBatch.Scope())
                {
                    var bleedDrawPos = new Vector2(position.X, position.Y);
                    var bleedDrawRect = new Rectangle(-(bleedPixels % 12), 0, bleedPixels, 12);
                    var bleedDrawOrigin = new Vector2(bleedPixels, 0);
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    Main.spriteBatch.Draw(CalamityUIResourceSets.MPManaBurn.Bar.Value, bleedDrawPos, bleedDrawRect, Color.White, 0, bleedDrawOrigin, 1, SpriteEffects.None, 1);
                    Main.spriteBatch.End();
                }
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
}
